---
title: Secret Store
description: Save credentials, use secrets in scripts and manage script access.
---

# Secret Store

Keep passwords, tokens and keys out of your scripts: save them once in **Tools → Secret Store**
and read them by name.

```csharp
var token = Util.GetSecret("billing-api");
```

<figure markdown="span">
  [![The Secret Store window: three secrets by name, the selected one's value hidden](assets/screenshots/secret-store_light.png#only-light)](assets/screenshots/secret-store_light.png "Open full-size screenshot")
  [![The Secret Store window: three secrets by name, the selected one's value hidden](assets/screenshots/secret-store.png#only-dark)](assets/screenshots/secret-store.png "Open full-size screenshot")
</figure>

The Secret Store window lists the names of your secrets. Select one to change its name or value;
the value stays hidden until you tick **Show**. **Add** and **Remove** change the list, **Save**
saves it and **Cancel** drops your changes. Names must differ, letter case aside, and
`Util.GetSecret` finds them the same way. If another Invoklet window changed the store meanwhile,
**Save** says so instead of overwriting it.

The secrets belong to you on this computer and live outside every script, in `secrets.json` in
Invoklet's data folder (see [Where Invoklet stores data](reference.md#where-invoklet-stores-data)). On
Windows the values are encrypted for your Windows account. If a secret is missing, Invoklet
can prompt for its value. If no value is supplied, `Util.GetSecret` throws `KeyNotFoundException`.
The command-line runner reads the same store. To use another file, for example on a build server, set the
`INVOKLET_SECRETS_FILE` environment variable to its path.

On macOS and Linux, Invoklet uses Keychain or the Secret Service to store the encryption key. If
the keychain is unavailable, Invoklet warns that a new or already unencrypted store is saved as
plain text with permissions limited to your account. If an encrypted store's key is unavailable,
saving fails and leaves the existing file intact.

When a script calls `Util.GetSecret`, Invoklet or the CLI returns the requested value without
passing the encryption key to the script. Starting the app or listing secret names does not
require unlocking the keychain.

## Script access

The first time a script asks for a secret, or saves one with `Util.SetSecret`, Invoklet asks whether it may: **Don't Allow** (the
script gets `UnauthorizedAccessException`, and the next run asks again), **Allow Once** (until Invoklet is closed) or **Always Allow**
(remembered for that file in Invoklet's settings). Allow only a script you trust: a script can send what it reads anywhere. The
scripts you always allowed are listed in **Tools → Secret Store** under **Scripts allowed to use secrets**; **Revoke** takes a
script's access away at once, and it is asked again the next time. Stopping a script while the question is open closes it. The
command-line runner doesn't ask, since you started the script yourself.
