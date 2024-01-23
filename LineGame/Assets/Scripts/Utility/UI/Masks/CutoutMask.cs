using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CutoutMask : Image
{
	public override Material materialForRendering
	{ 
		get
		{
			Material copyMat = new Material(base.materialForRendering);
			copyMat.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
			return copyMat;
		}
	}
}
