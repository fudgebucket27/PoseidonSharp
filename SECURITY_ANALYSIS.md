# PoseidonSharp — Security Analysis

**Scope:** all of `PoseidonSharp/`, `PoseidonConsole/`, `PoseidonTests/`, `README.md` at commit `a257628`.
**Context:** this library produces EdDSA (BabyJubJub) signatures over Poseidon hashes that authenticate
requests to the Loopring L2 API. It handles long-term L2 private keys, so signing-key confidentiality and
signature soundness are the primary security properties.

**Method:** manual review of all 2,022 lines, plus an independent Python reimplementation of `Point.cs`
and `Eddsa.Verify` used to execute the proof-of-concept attacks in F2. No .NET SDK was available in the
review environment, so the PoCs exercise a faithful port of the curve arithmetic rather than the C# build.

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| F1 | **Critical** | Live Ethereum L1 private key committed to the repo and published in the README | `README.md:47`, `PoseidonTests/L2KeyGenerationTests.cs:24` |
| F2 | **High** | `Verify` performs no validation of `A`, `R` or `S` — universal forgery, malleability, invalid-curve input | `Eddsa.cs:90-101` |
| F3 | **High** | `PrecomputedPointA` / `LastPrivateKey` are `static` — cross-key contamination and data races silently produce invalid signatures | `Eddsa.cs:17-57` |
| F4 | **High** | Scalar multiplication is not constant-time — timing/cache side channel on the nonce and the private key | `Point.cs:24-43`, `LoopringL2KeyGenerator.cs:138-152` |
| F5 | **Medium** | `Verify` ignores `signedMessage.Message` — the field is never authenticated | `Eddsa.cs:94,97` |
| F6 | **Medium** | Poseidon has no padding/domain separation and does not reduce inputs — trivial collisions | `Poseidon.cs:292-302` |
| F7 | **Medium** | All input and security-parameter validation uses `Debug.Assert` — compiled out of Release builds | `Poseidon.cs:29-71,287-291` |
| F8 | **Medium** | No zeroization of key material; secrets held as immutable `string` | `Eddsa.cs`, `LoopringL2KeyGenerator.cs` |
| F9 | **Low** | Attacker-supplied signature strings can parse to negative `Integer`s | `EddsaHelper.cs:12-24` |
| F10 | **Low** | Modular inverse returns garbage instead of throwing on non-invertible input | `LoopringL2KeyGenerator.cs:118-136` |
| F11 | **Low** | `Poseidon` silently ignores caller-supplied constants, then null-derefs | `Poseidon.cs:27-94` |
| F12 | **Low** | `Sign(object)` uses an unchecked cast and a base point inconsistent with `A` | `Eddsa.cs:59-71` |
| F13 | Info | Dated dependencies, obsolete crypto APIs, upstream-inherited Poseidon quirks | various |

---

## F1 — Live Ethereum private key committed to the repository (Critical)

`PoseidonTests/L2KeyGenerationTests.cs:24` and `README.md:47` contain a 32-byte secp256k1 private key in
plaintext:

```csharp
var l1PrivateKey = "8fe76a950a68a723e9ecd0c256045266e94ed5a1e846ca2112a9ecb61c1d28db"; //L1 private key
var ethAddress   = "0x991B6fE54d46e5e0CEEd38911cD4a8694bed386A"; //eth address
```

This is not a placeholder. Deriving the address from the key confirms it:

```
privkey 8fe76a95... -> 0x991B6fE54d46e5e0CEEd38911cD4a8694bed386A
```

which matches the `ethAddress` asserted in the test exactly. The key controls that mainnet account and,
via `GenerateL2KeyDetails`, the Loopring L2 account whose secret key the test also asserts
(`0x03630456…`, `L2KeyGenerationTests.cs:33`). Anyone who has read the README can sign L1 transactions and
L2 API requests for that account.

Three further L2 private keys are hardcoded at `PoseidonTests/VerifyTests.cs:16-18`, `SigningTests.cs`, and
`PoseidonConsole/Program.cs:38`. The comments say they were "unpaired from the real account", which
mitigates but does not eliminate the exposure — unpairing does not invalidate the underlying key material,
and it was never true of the L1 key.

The key is present in git history (it appears in the diff at least twice) and has been shipped inside the
published NuGet package's README since at least v1.0.7.

**Remediation**

1. Treat `0x991B6fE54d46e5e0CEEd38911cD4a8694bed386A` as fully compromised. Move any remaining L1 or L2
   balance to a fresh address now, and reset the Loopring L2 key for that account. Do this before touching
   the source, because history rewriting does not un-publish anything.
2. Replace the README example and the test fixture with a key generated purely for documentation, and label
   it as such — or better, load fixtures from an untracked file / environment variable.
3. Purge from history (`git filter-repo --replace-text`) and force-push, then republish the NuGet package.
   Note this is cleanup, not remediation: the key must be assumed permanently public.
4. Enable GitHub secret scanning and push protection on the repository to prevent recurrence.

---

## F2 — `Verify` validates nothing (High)

```csharp
public bool Verify(SignedMessage signedMessage)
{
    var A = signedMessage.A;
    var sig = signedMessage.Signature;
    var B = (/* hardcoded generator */);
    var lhs = Point.Multiply(sig.S, B);
    var hashPublic = Integer.Parse(HashPublic(sig.R, A, Integer.Parse(OriginalHash.ToString())).ToString());
    var rhs = Point.Add(sig.R, Point.Multiply(hashPublic, A));
    return lhs == rhs;
}
```

There is no check that `A` and `R` lie on the curve, no check that `A` lies in the prime-order subgroup, and
no check that `S` is in range `[0, L)`. Each omission is independently exploitable. A Python port of
`Point.Add` / `Point.Multiply` / `Verify` confirms all four results:

**a) Universal forgery under the identity public key.** `A = R = (0,1)`, `S = 0` verifies against *any*
message, because `0·B = O` and `O + t·O = O` for every `t`:

```
Verify(A=(0,1), R=(0,1), S=0, t=0)                       -> True
Verify(A=(0,1), R=(0,1), S=0, t=12345)                   -> True
Verify(A=(0,1), R=(0,1), S=0, t=2^200+7)                 -> True
Verify(A=(0,1), R=(0,1), S=0, t=p-1)                     -> True
```

Any caller that accepts a counterparty-supplied `A` can be handed a signature that validates for arbitrary
content.

**b) Signature malleability.** `B` has order `L`, so `S` and `S + kL` are indistinguishable to the equation:

```
original S verifies:  True
S + 1*L verifies:     True   (distinct encoding, 252 bits)
S + 2*L verifies:     True   (distinct encoding, 253 bits)
S + 3*L verifies:     True   (distinct encoding, 253 bits)
```

Every signature has unbounded distinct valid encodings. Any logic that treats a signature string as a unique
identifier — replay caches, dedup keys, idempotency tokens, on-chain nullifiers — is defeated.

**c) Small-subgroup points accepted.** The order-2 torsion point `(0, -1)` is on the curve but outside the
prime-order subgroup, and `Verify(A=(0,-1), R=(0,1), S=0, t)` returns `True` for every even `t` — roughly
half of all messages, with no knowledge of any secret.

**d) Off-curve points accepted.** `(5,7)` is not on BabyJubJub, yet `Point.Multiply` happily processes it and
returns a point, performing arithmetic in a group the security argument does not cover.

**Remediation.** Before using `A` or `R`, reject anything that fails:

- `0 <= x,y < p` and `a·x² + y² == 1 + d·x²y²` (on-curve),
- `L·A == O` (prime-order subgroup) — or multiply by the cofactor 8 and reject the identity,
- `A != O` (reject the identity public key outright),
- `0 <= S < L` (canonical scalar).

Reject rather than normalize, so malleated encodings fail rather than silently succeed.

---

## F3 — Static precomputed public key shared across all instances (High)

```csharp
private static BigInteger LastPrivateKey { get; set; }
private static (Integer x, Integer y) PrecomputedPointA;

public Eddsa(BigInteger _originalHash, string _privateKey)
{
    ...
    if (privateKeyBigInteger != LastPrivateKey) { LastPrivateKey = ...; ResetPreComputedPointA(); }
    PrivateKey = privateKeyBigInteger;
    if (PrecomputedPointA == default) { PrecomputedPointA = Point.Multiply(PrivateKey, B); }
}
```

The cache key and the cached value are process-global while `PrivateKey` is per-instance, so the two can
disagree.

**Sequential contamination.** Construct `e1` with key `K1`, then `e2` with key `K2`. Constructing `e2`
overwrites `PrecomputedPointA` with `K2·B`. Calling `e1.Sign()` afterwards computes
`t = HashPublic(R, K2·B, m)` while `S = r + K1·t` — a signature bound to the wrong public key. It does not
throw; it returns a well-formed hex string that Loopring rejects. Any application that holds more than one
account — a trading bot, a multi-wallet client, a service signing on behalf of several users — hits this on
every interleaved call, and each failure looks like an opaque API error rather than a library bug.

**Race condition.** The pattern is check-then-act with no synchronization. Two threads constructing with
different keys can interleave so that thread A's `Point.Multiply` (a multi-millisecond operation) finishes
*after* thread B has stored its own value, leaving `PrecomputedPointA` set to A's key while B signs.
Separately, `(Integer, Integer)` is a `ValueTuple` of structs and its assignment is not atomic — a
concurrent reader can observe a torn value mixing two different points, or a half-written `Integer` whose
magnitude array and sign are inconsistent.

**Remediation.** Make `PrecomputedPointA` an instance field, computed once in the constructor. If a cache
across instances is genuinely wanted for performance, key it correctly and make it thread-safe — e.g. a
`ConcurrentDictionary<BigInteger, (Integer, Integer)>` or `Lazy<T>` per instance — but note the current
design saves one scalar multiplication per signature at the cost of correctness. Also delete
`ResetPreComputedPointA()` from the public API; it lets any caller invalidate every other caller's state.

---

## F4 — Non-constant-time scalar multiplication (High)

```csharp
public static (Integer, Integer) Multiply(Integer scalar, (Integer x, Integer y) _points)
{
    while (scalar != 0)
    {
        if ((scalar & one) != 0) { a = Add(a, p); }   // branch on a secret bit
        p = Add(p, p);
        scalar = IntegerFunctions.DivRem(scalar, 2, out scalar);
    }
}
```

Textbook double-and-add. The conditional `Add` executes only when the current secret bit is 1, so both
runtime and memory-access pattern are linear in the Hamming weight of the scalar, and the loop count leaks
its bit length. `Point.Add` itself calls `IntegerFunctions.ModInv` — an extended-Euclid inversion whose
iteration count depends on its operands — and NeinMath's `Integer` makes no constant-time claims.
`LoopringL2KeyGenerator.mulPointEscalar` has the identical structure.

Two secrets flow through this routine. `Point.Multiply(PrivateKey, B)` in the constructor leaks the
long-term key. More sharply, `Eddsa.Sign` computes `R = Point.Multiply(r, B)` where `r` is the per-message
nonce — and recovering `r` for a single signature yields the private key algebraically from
`k = (S - r)·t⁻¹ mod L`. Even a partial leak of `r` across several signatures is enough for a lattice
attack on the hidden number problem.

Exploitation requires a local or co-resident attacker, or a remote timing oracle where signing latency is
observable — a hosted signing service, a shared CI runner, a browser-adjacent process. That is a narrower
threat model than F1–F3, but the consequence is full key recovery, and a signing library should not depend
on the deployment being single-tenant.

**Remediation.** Use a Montgomery ladder or fixed-window multiplication with unconditional point additions,
and complete (exception-free) twisted-Edwards addition formulas. Replace the per-addition modular inversion
with projective/extended coordinates and one inversion at the end — this is a large performance win as well.
Realistically, the durable fix is to delegate to a reviewed BabyJubJub implementation rather than maintain
constant-time field arithmetic in this repo.

---

## F5 — `Verify` ignores the message it is given (Medium)

```csharp
var msg = signedMessage.Message;                                        // never used
var hashPublic = HashPublic(sig.R, A, Integer.Parse(OriginalHash.ToString()));   // uses the instance field
```

`msg` is dead. Verification is performed against `OriginalHash`, supplied to the `Eddsa` constructor,
regardless of what `SignedMessage.Message` contains. The practical effect is that `SignedMessage.Message` is
an unauthenticated field: it can be set to anything and `Verify` still returns `true`. Downstream code that
reads `signedMessage.Message` after a successful verification — a natural thing to do, and what the type
name implies — is trusting attacker-controlled data.

This also explains why the API is awkward: `Verify` is an instance method on a class whose constructor
demands a private key, so verifying someone else's signature requires fabricating a private key.

**Remediation.** Use `msg` in the `HashPublic` call, and move `Verify` to a static method taking
`(SignedMessage)` — or a free function on a `PublicKey` type — so verification does not require a secret.

---

## F6 — Poseidon has no padding or domain separation (Medium)

```csharp
BigInteger[] state = new BigInteger[T];
for (long i = 0; i < T; i++) state[i] = 0;
for (int i = 0; i < inputs.Length; i++) state[i] = inputs[i];
```

The state is zero-filled and inputs are copied into the low positions with no length encoding and no
capacity separator. Two consequences follow directly:

- **Trailing-zero collision.** `CalculatePoseidonHash([x])` and `CalculatePoseidonHash([x, 0])` build a
  byte-identical state array and therefore return the same hash. So do `[x, 0, 0]`, `[x, y]` vs `[x, y, 0]`,
  and so on for any input vector extended with zeros.
- **Unreduced-input collision.** Inputs are never reduced modulo the field. The first thing that happens to
  `state[i]` is `+ C` followed by `BigInteger.ModPow(state[i], 5, p)` in the first (full) S-box round, which
  reduces mod `p`. Therefore `x` and `x + p` — and `x` and `x - p` — produce identical hashes. Negative
  inputs are likewise silently folded into the field.

For the library's own use, `HashPublic` passes a fixed 5-element vector of already-reduced field elements,
so neither collision is reachable there. But `CalculatePoseidonHash` is `public` and the README documents it
as a general-purpose hashing entry point, so any consumer hashing variable-length data or values that are
not pre-reduced inherits a collision-finding shortcut.

**Remediation.** For Loopring compatibility the permutation itself must not change. Add a validating wrapper
instead: reject inputs `>= p` or `< 0`, and either fix the arity per instance or encode the input length
into the state before permuting. At minimum, document that the function is only safe for fixed-arity,
pre-reduced inputs.

---

## F7 — Validation via `Debug.Assert` disappears in Release builds (Medium)

`Poseidon`'s entire validation surface — the round-count/parity checks (`Poseidon.cs:29-30`), the three
Gröbner and interpolation attack-resistance bounds (`:69-71`), and the `inputs.Length < T` arity check
(`:290`) — is expressed with `Debug.Assert`. These are compiled out when `DEBUG` is not defined, which is
every NuGet consumer of this package.

The attack-ratio assertions exist precisely to stop a caller from instantiating Poseidon with round counts
too low to resist algebraic attacks. In a Release build a caller can pass `new Poseidon(6, 2, 1, ...)` and
receive a working but cryptographically broken hash function with no diagnostic. The arity check is less
severe — an oversized input array throws `IndexOutOfRangeException` instead — but the failure is an obscure
runtime exception rather than a clear argument error.

**Remediation.** Convert every security-relevant assertion to an unconditional `throw new
ArgumentException(...)` / `ArgumentOutOfRangeException`. Keep `Debug.Assert` only for genuine internal
invariants.

---

## F8 — Key material is never zeroized (Medium)

Private keys enter as `string` (`Eddsa` constructor, `LoopringL2KeyGenerator.GenerateL2KeyDetails`) and are
returned as `string` (`KPair.SecretKey`, `RipKeyAppart`). .NET strings are immutable and heap-allocated, so
every key persists in managed memory until collected, can be duplicated by GC compaction, and can be written
to the page file or captured in a crash dump. The intermediate `BigInteger` / `Integer` values, the SHA-256
seed buffer in `RipKeyAppart` (`:52`), and the SHA-512 input in `HashPrivateKey` (`:124-127`) are likewise
left in memory.

`static BigInteger LastPrivateKey` (F3) makes this materially worse: it holds a copy of the most recently
used private key for the lifetime of the process, reachable from a static root, long after every `Eddsa`
instance is gone.

**Remediation.** Accept and handle keys as `byte[]` or `ReadOnlySpan<byte>`, `CryptographicOperations.ZeroMemory`
intermediate buffers after use, and drop `LastPrivateKey` entirely. Perfect scrubbing is not achievable in
managed .NET, but minimizing lifetime and eliminating the static copy are both cheap.

---

## F9 — Signature parsing can produce negative scalars (Low)

```csharp
var r = IntegerConverter.FromHexString(pureHexSig.Substring(0, 64));
```

`IntegerConverter.FromHexString` interprets its input as two's complement — the codebase acknowledges this
everywhere else by prepending a `"0"` when `Sgn() == -1` (`Eddsa.cs:130-134`, `EddsaHelper.cs:29-33`,
`SHA256Helper.cs:27-31`). `SignatureStringToSignatureObject` omits that normalization.

Signatures produced by this library never trigger it: `Rx`, `Ry` and `S` are all bounded by `p` and `E`,
both just under `2²⁵⁴`, so the leading hex digit is at most `2`. That is also why the tests never caught it.
An *attacker-supplied* 194-character signature string has no such bound — a leading nibble `>= 8` yields a
negative `Integer`, which then reaches `Point.Multiply`, where `scalar & 1` and `DivRem(scalar, 2)` on a
negative value are outside the routine's design. Verification results are undefined for such input.

**Remediation.** Apply the same leading-zero normalization used elsewhere, then explicitly range-check each
component (`Rx, Ry < p`, `S < L`) before use — which F2's remediation covers.

---

## F10 — Modular inverse fails silently (Low)

```csharp
private static BigInteger Pinv(BigInteger a, BigInteger p)
{
    ... while (newr > 0) { ... }
    if (t < 0) t += p;
    return t;                 // no check that gcd(a,p) == 1
}
```

Unlike `Point.ExtendedEuclideanInverse` (`Point.cs:120-121`), which correctly throws when `r > 1`, `Pinv`
never verifies invertibility. For `a ≡ 0 (mod p)` the loop does not execute and it returns `0`, so `Pdiv`
yields `0` and `addPoint` returns a mathematically meaningless point that propagates through
`mulPointEscalar` into a derived public key — wrong output where an exception is wanted. `Point.Add`'s use
of `IntegerFunctions.ModInv` has the same exposure, depending on NeinMath's behaviour for non-invertible
input.

**Remediation.** Check `r == 1` and throw otherwise, matching `ExtendedEuclideanInverse`. Complete Edwards
addition formulas (F4) remove the failure mode at the source.

---

## F11 — `Poseidon` ignores caller-supplied constants (Low)

The constructor accepts `_constantsC` and `_constantsM`, but only assigns `ConstantsC` / `ConstantsM` inside
the `if (… == null)` branches (`Poseidon.cs:74-94`). Passing non-null constants leaves both properties
`null`, and `CalculatePoseidonHash` then throws `NullReferenceException` on the `foreach` over `ConstantsC`.
Related dead code in the same constructor: `constantsCseedBytes` and `constantsMseedBytes` are computed and
discarded (`:77`, `:92`), and `nConstraints` is computed and never used (`:96-104`).

**Remediation.** Either honour the parameters or remove them from the signature.

---

## F12 — `Sign(object)` weak typing and generator mismatch (Low)

```csharp
public string Sign(object _points = null)
{
    if (_points != null) { B = ((Integer x, Integer))_points; }   // unchecked cast
    ...
    (Integer x, Integer y) A = PrecomputedPointA;                 // computed with the *hardcoded* B
    (Integer x, Integer y) R = Point.Multiply(r, B);              // uses the *caller's* B
```

Two problems. The `object` parameter with an unchecked tuple cast turns a caller mistake into an
`InvalidCastException` at signing time instead of a compile error. And when a custom base point *is* passed,
`R` is derived from it while `A` still comes from the precomputed value over the hardcoded generator, so the
resulting signature is internally inconsistent and cannot verify.

**Remediation.** Change the parameter to `(Integer x, Integer y)?`, and either derive `A` from the same base
point or remove the parameter — nothing in the codebase uses it.

---

## F13 — Informational

- **Dated dependencies.** `NeinMath 1.5.1` and `SauceControl.Blake2Fast 2.0.0` are the runtime dependencies;
  `Nethereum.Signer 4.16.0`, `MSTest 2.2.3` and `Microsoft.NET.Test.Sdk 16.9.4` are test-only and several
  years behind. Worth running `dotnet list package --vulnerable --include-transitive` in CI. Note that
  NeinMath is a general-purpose bignum library with no side-channel resistance goals (see F4).
- **Obsolete crypto APIs.** `SHA256Managed` (`SHA256Helper.cs:18`, `LoopringL2KeyGenerator.cs:51`) and
  `SHA512Managed` (`Eddsa.cs:126`) are obsolete (SYSLIB0021) and the instances are never disposed. Use
  `SHA256.Create()` / `SHA512.HashData()`. No security impact, but they will warn or break on future
  targets.
- **Upstream-inherited Poseidon quirks.** `SNARK_SCALAR_FIELD % 2 == 3` (`Poseidon.cs:50`) can never be
  true, making the `e == 3` branch dead. And `CalculatePoseidonHash` adds a single round constant to *all*
  state lanes (`:307-310`) rather than `t` distinct constants per round, which deviates from the Poseidon
  specification. Both faithfully reproduce Loopring's `ethsnarks` reference implementation, so **neither
  should be "fixed"** — changing either breaks wire compatibility with Loopring. Flagged so that a future
  reader does not mistake them for local bugs, and so the deviation from the published Poseidon security
  analysis is on record.
- **`HashPrivateKey` padding does not do what its comment says.** `Eddsa.cs:115-119` claims to "pad out byte
  array to 32 bytes" but appends exactly one zero byte regardless of length. The resulting map from message
  hash to nonce input remains injective, so this does not create nonce reuse — but the comment is
  misleading, and the construction concatenates key and message without length framing, which is fragile
  against future edits. Any change here alters signatures and breaks Loopring compatibility.
- **Console demo.** `PoseidonConsole/Program.cs:43` divides by `sw.ElapsedMilliseconds / 1000`, throwing
  `DivideByZeroException` on any run under one second. Cosmetic.

---

## Suggested order of work

1. **F1** — rotate the exposed keys. Independent of all code changes and the only finding with an active,
   ongoing exposure.
2. **F3** — make `PrecomputedPointA` per-instance. Small, self-contained, and fixes silent signature
   corruption that users hit today.
3. **F2 + F5 + F9** — rewrite `Verify` with full point and scalar validation, use the supplied message, and
   make it static. These share one code path.
4. **F7 + F6 + F11** — replace `Debug.Assert` with real exceptions and validate Poseidon inputs.
5. **F4 + F8** — constant-time scalar multiplication and key-material hygiene. The largest effort; consider
   adopting a reviewed BabyJubJub implementation rather than hardening this one.

Regression safety: the existing tests in `PoseidonTests/` pin exact signature and hash outputs against
Loopring, so they are a solid guard for items 2–5. None of the recommended changes should alter any asserted
value — if one does, the change has broken wire compatibility. Note that the fixtures currently embed the
key material from F1 and will need regenerating as part of that rotation.
