// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

/* Scoped to the one type that talks to WMI rather than the whole module, so that platform
 * compatibility is still checked everywhere else.
 */

[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This program only ships on Windows", Scope = "type", Target = "~T:NVUpdateManager.Core.WmiSystemHardwareInfo")]
