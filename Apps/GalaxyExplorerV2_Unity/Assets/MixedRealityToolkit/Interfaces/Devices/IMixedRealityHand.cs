﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices
{
    /// <summary>
    /// Hand definition, used to provide access to hand joints and other data.
    /// </summary>
    public interface IMixedRealityHand : IMixedRealityController
    {
        /// <summary>
        /// Get the current pose of a hand joint.
        /// </summary>
        /// <remarks>
        /// Hand bones should be oriented along the Z-axis, with the Y-axis indicating the "up" direction,
        /// i.e. joints rotate primarily around the X-axis.
        /// </remarks>
        bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose);
    }
}