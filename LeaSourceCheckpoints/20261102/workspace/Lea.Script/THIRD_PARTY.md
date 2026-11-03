# Third-Party Notices

The optional JavaScript runtime uses these pinned packages. It does not add
them to the base `Lea.Engine` runtime. Full redistribution notices accompany
the test package in `licenses`.

| Package | Version | License | Source |
| --- | --- | --- | --- |
| Jint | 4.16.4 | BSD-2-Clause | https://github.com/sebastienros/jint/tree/v4.16.4 |
| Acornima | 1.7.0 | BSD-3-Clause | https://github.com/adams85/acornima/tree/401bd62d8aeb9f7cbe7b6147937a87e0c71747a8 |
| System.Buffers | 4.5.1 | MIT | https://www.nuget.org/packages/System.Buffers/4.5.1 |
| System.Memory | 4.5.5 | MIT | https://www.nuget.org/packages/System.Memory/4.5.5 |
| System.Numerics.Vectors | 4.5.0 | MIT | https://www.nuget.org/packages/System.Numerics.Vectors/4.5.0 |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | MIT | https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0 |

Framework reference-assembly packages in the lock files are build-time inputs,
not part of the shipped executable's runtime files. Lea's source backup keeps
our adapter, tests, bootstrap, and exact dependency lock files; it does not
vendor the complete third-party repositories.
