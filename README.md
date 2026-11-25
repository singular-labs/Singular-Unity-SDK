# Singular SDK - Unity Package

This is a Unity package for Singular SDK for Unity.

Documentation for this Unity package is provided as links to the Singular Unity SDK Documentation and websites.

## Documentation

* [Singular.net Main Page](https://www.singular.net)
* [Singular Unity SDK Documentation](https://support.singular.net/hc/en-us/articles/360037635452-Unity-SDK-Integration-Guide)
* [Singular Unity SDK Changelog](https://support.singular.net/hc/en-us/articles/360040639152-Unity-SDK-Change-Log)

## Standard and Kids editions

Singular publishes this package in two editions, built from the same source:

| Edition | Package name | Install from Git URL |
|---|---|---|
| Singular | `singular-unity-package` | `https://github.com/singular-labs/Singular-Unity-SDK.git#<version>` |
| Singular Kids | `singular-unity-kids-package` | `https://github.com/singular-labs/Singular-Unity-SDK.git#<version>-kids` |

Apps in a kids category use **Singular Kids**. Compared with the standard edition, it:

* depends on the Singular Kids native SDKs (`singular_kids_sdk` on Android, `Singular-Kids-SDK` on iOS),
* does not contain the APIs the Kids SDKs don't support: `GetIDFA`, `SetIMEI`, `SetLimitAdvertisingIdentifiers`,
  `SetCollectIpv6Address`, `SetUserDetails` / `ClearUserDetails` / `SingularUserDetails`, and the `collectOAID`,
  `limitAdvertisingIdentifiers` and `collectIpv6Address` options,
* does not link `AdSupport.framework` on iOS or add the `AD_ID` permission on Android,
* reports its version with a `-KIDS` suffix.

The rest of the API is the same. To switch editions, replace one package with the other; no scripting define
symbol is needed.
