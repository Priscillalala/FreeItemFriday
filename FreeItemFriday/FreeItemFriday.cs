using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using GrooveSharedUtils;
using GrooveSharedUtils.Frames;
using UnityEngine;

[module: UnverifiableCode]
# pragma warning disable
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
# pragma warning restore
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace FreeItemFriday
{
    public class FreeItemFriday : ModPlugin<FreeItemFriday>
    {
        public override string ModName => "FreeItemFriday";

        public override string AuthorName => "groovesalad";

        public override string VersionNumber => "1.1.0";
        public override IEnumerator LoadContent()
        {
            yield return new ExpansionFrame
            {
                name = "FreeItemFriday",
                icon = Assets.bundle.LoadAsset<Sprite>("texFreeItemFridayExpansionIcon")
            };
        }
    }
}
