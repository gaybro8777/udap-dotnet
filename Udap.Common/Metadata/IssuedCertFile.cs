﻿#region (c) 2022 Joseph Shook. All rights reserved.
// /*
//  Authors:
//     Joseph Shook   Joseph.Shook@Surescripts.com
// 
//  See LICENSE in the project root for license information.
// */
#endregion


namespace Udap.Common.Metadata;

public class IssuedCertFile
{
    /// <summary>
    /// Relative path
    /// </summary>
    public string? FilePath { get; set; }

    public string? Password { get; set; }

}