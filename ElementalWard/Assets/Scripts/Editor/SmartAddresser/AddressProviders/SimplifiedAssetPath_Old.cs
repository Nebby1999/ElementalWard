using System;
using System.Collections;
using System.Collections.Generic;
using SmartAddresser.Editor.Core.Models.LayoutRules.AddressRules;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Nebula.Editor.SmartAddresser
{
	[CreateAssetMenu(menuName = "Nebula/Editor/AddressProviders/SimplifiedAssetPath")]
    public class SimplifiedAssetPath_Old : AddressProviderAsset
    {
		public override string GetDescription()
		{
			return "Regular asset path but without \"Assets/\"";
		}

		public override string Provide(string assetPath, Type assetType, bool isFolder)
		{
			return assetPath.StartsWith("Assets/") ? assetPath.Substring("Assets/".Length) : assetPath;
		}

		public override void Setup()
		{
		}
    }
}
