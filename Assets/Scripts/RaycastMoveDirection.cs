// Copyright (c) 2017, Timothy Ned Atton.
// All rights reserved.
// nedmakesgames@gmail.com
// This code was written while streaming on twitch.tv/nedmakesgames
//
// This file is part of Raycast Platformer.
//
// Raycast Platformer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Raycast Platformer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Raycast Platformer.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using System.Collections;

public class RaycastMoveDirection {

    private Vector2 raycastDirection;
    private Vector2[] offsetPoints;
    private LayerMask layerMask;
    private float addLength;

    public RaycastMoveDirection(Vector2 start, Vector2 end, Vector2 dir, LayerMask mask, Vector2 parallelInset, Vector2 perpendicularInset) {
        this.raycastDirection = dir;
        this.offsetPoints = new Vector2[] {
            start + parallelInset + perpendicularInset,
            end - parallelInset + perpendicularInset,
        };
        this.addLength = perpendicularInset.magnitude;
        this.layerMask = mask;
    }

    public float DoRaycast(Vector2 origin, float distance) {
        float minDistance = distance;
        foreach(var offset in offsetPoints) {
            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, distance + addLength, layerMask);
            if(hit.collider != null) {
                MoveThroughPlatform mtp = hit.collider.GetComponent<MoveThroughPlatform>();
                if(mtp == null || Vector2.Dot(raycastDirection, mtp.permitDirection) < mtp.dotLeeway) {
                    minDistance = Mathf.Min(minDistance, hit.distance - addLength);
                }
            }
        }
        return minDistance;
    }

    private RaycastHit2D Raycast(Vector2 start, Vector2 dir, float len, LayerMask mask) {
        //Debug.Log(string.Format("Raycast start {0} in {1} for {2}", start, dir, len));
        Debug.DrawLine(start, start + dir * len, Color.blue);
        return Physics2D.Raycast(start, dir, len, mask);
    }
}
