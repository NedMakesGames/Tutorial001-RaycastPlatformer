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

public class RaycastCheckTouch {

    private Vector2 raycastDirection;
    private Vector2[] offsetPoints;
    private LayerMask layerMask;
    private float raycastLen;

    public RaycastCheckTouch(Vector2 start, Vector2 end, Vector2 dir, LayerMask mask, Vector2 parallelInset, Vector2 perpendicularInset, float checkLength) {
        this.raycastDirection = dir;
        this.offsetPoints = new Vector2[] {
            start + parallelInset + perpendicularInset,
            end - parallelInset + perpendicularInset,
        };
        this.raycastLen = perpendicularInset.magnitude + checkLength;
        this.layerMask = mask;
    }

    public Collider2D DoRaycast(Vector2 origin) {
        foreach(var offset in offsetPoints) {
            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, raycastLen, layerMask);
            if(hit.collider != null) {
                return hit.collider;
            }
        }
        return null;
    }

    private RaycastHit2D Raycast(Vector2 start, Vector2 dir, float len, LayerMask mask) {
        //Debug.Log(string.Format("Raycast start {0} in {1} for {2}", start, dir, len));
        Debug.DrawLine(start, start + dir * len, Color.red);
        return Physics2D.Raycast(start, dir, len, mask);
    }
}
