﻿using UnityEngine;

namespace Libs.Helpers
{
    public static class TransformExtensions
    {
        public static void ClearExistingElementsInParent(this Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

}