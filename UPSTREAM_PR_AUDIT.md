# Upstream Pull Request Integration Audit

Upstream repository: https://github.com/jayugg/SmithingPlus

Fork integration branch: `agent/process-upstream-prs`

Baseline: upstream `master` commit `241b9f294eb3cde29a9672b5ca2a52cc4d9ff5fd`.

This audit records the disposition of every upstream pull request visible when the fork was established. A pull request marked as superseded or selectively extractable must not be applied wholesale without a new review.

| PR | Upstream state | Fork disposition | Reason |
|---|---|---|---|
| [#5](https://github.com/jayugg/SmithingPlus/pull/5) | Closed, unmerged | Superseded | Historical tool-recovery correction duplicated by #6 and overtaken by later recipe/API changes. |
| [#6](https://github.com/jayugg/SmithingPlus/pull/6) | Closed, unmerged | Superseded | Same essential correction as #5; current recovery code no longer matches this patch. |
| [#36](https://github.com/jayugg/SmithingPlus/pull/36) | Closed, unmerged | Superseded; regression test required | Old chisel null guard. Later chisel correction #110 is already in the baseline. |
| [#60](https://github.com/jayugg/SmithingPlus/pull/60) | Closed, unmerged | Superseded; regression test required | Based on 1.5.7 handbook/material code that was later substantially reworked. |
| [#62](https://github.com/jayugg/SmithingPlus/pull/62) | Merged | Present in baseline | Bit-recovery method extraction for external compatibility. |
| [#76](https://github.com/jayugg/SmithingPlus/pull/76) | Closed, unmerged | Superseded | Partial 1.21 release-candidate port superseded by #77 and later releases. |
| [#77](https://github.com/jayugg/SmithingPlus/pull/77) | Merged | Present in baseline | Main 1.21 release-candidate upgrade. |
| [#78](https://github.com/jayugg/SmithingPlus/pull/78) | Merged | Historical only | Final 1.20.12 maintenance work; the fork targets 1.22.5 and later. |
| [#79](https://github.com/jayugg/SmithingPlus/pull/79) | Merged | Present in history | 1.21 release transition; GitHub reports no remaining changed files against its target. |
| [#85](https://github.com/jayugg/SmithingPlus/pull/85) | Closed, unmerged | Superseded | Hard-coded mold-unit feature with a known handbook issue; superseded by #101's dynamic system. |
| [#101](https://github.com/jayugg/SmithingPlus/pull/101) | Merged | Present in baseline | Major 1.8 architecture and dynamic mold-unit rework. |
| [#110](https://github.com/jayugg/SmithingPlus/pull/110) | Merged | Present in baseline | Chisel spam-click null correction. |
| [#116](https://github.com/jayugg/SmithingPlus/pull/116) | Closed, unmerged | Superseded by #130 | Earlier partial configuration synchronisation change. |
| [#118](https://github.com/jayugg/SmithingPlus/pull/118) | Merged | Present in baseline | Belarusian translation. |
| [#121](https://github.com/jayugg/SmithingPlus/pull/121) | Closed, unmerged | Superseded by #125 | Community 1.22 release-candidate port. |
| [#125](https://github.com/jayugg/SmithingPlus/pull/125) | Merged | Present in baseline | Official 1.22 update and fork baseline. |
| [#130](https://github.com/jayugg/SmithingPlus/pull/130) | Open upstream | Integrated through fork PR #1 | Client/server configuration split and synchronisation. Merge commit `704c5f5217bd7e927dff1859b6fd87fc4da5dc28`. |
| [#131](https://github.com/jayugg/SmithingPlus/pull/131) | Open upstream | Staged as fork PR #2; selective extraction only | Conflicts with #130 and mixes caching with unrelated removals, assets, compatibility and version changes. The upstream description acknowledges a smithing-with-bits regression during porting. |
| [#132](https://github.com/jayugg/SmithingPlus/pull/132) | Open upstream | Integrated through fork PR #3 | Profiled reverse-index and material-cache correction. Merge commit `567cc617b654d91a03e8925dbc422bea2ec14faa`. |
| [#139](https://github.com/jayugg/SmithingPlus/pull/139) | Open upstream | Staged as fork PR #4; corrected manual port required | Conflicts with the integrated branch. Exact 1.22.5 destruction/cancellation semantics must be verified before recovery is moved. |

## Validation gate

No mod-domain or mod-identifier rename is permitted until:

1. all accepted changes pass the project coding-standard audit;
2. the project compiles against the exact Vintage Story 1.22.5 assemblies and .NET version;
3. cold and warm anvil interaction performance is measured;
4. Smith With Bits, cast heads, workable nuggets, handbook rendering, configuration synchronisation, chisel reclaim and broken-tool recovery receive runtime regression tests;
5. the separate SmithingPlusBugFix hotfix is removed during standalone-fork testing.
