// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class BoundingBoxHandler : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Size of scale handles in Bounding box.")]
        private Vector3 scaleHandleSize = new Vector3(0.08f, 0.08f, 0.08f);

        [SerializeField]
        [Tooltip("Size of rotate handles in Bounding box.")]
        private Vector3 rotateHandleSize = new Vector3(0.08f, 0.08f, 0.08f);

        [SerializeField]
        [Tooltip("Parent entity of bounding box automatic generated gameobjects.")]
        private GameObject ParentOfBBEntities = null;

        private void LateUpdate()
        {
            if (transform.lossyScale.x > GalaxyExplorerManager.Instance.ToolsManager.LargestZoom && transform.parent)
            {
                transform.localScale = GalaxyExplorerManager.Instance.ToolsManager.LargestZoom * Vector3.one / transform.parent.lossyScale.x;
            }
            else if (transform.lossyScale.x < GalaxyExplorerManager.Instance.ToolsManager.MinZoom && transform.parent)
            {
                transform.localScale = GalaxyExplorerManager.Instance.ToolsManager.MinZoom * Vector3.one / transform.parent.lossyScale.x;
            }
        }
    }
}