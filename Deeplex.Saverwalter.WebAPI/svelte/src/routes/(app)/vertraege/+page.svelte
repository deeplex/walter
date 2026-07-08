<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import { WalterVertraege, WalterHeader } from '$walter/components';
    import type {
        WalterVertragEntry,
        WalterVertragVersionEntry
    } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;

    // Leeres entry aktiviert das Inline-Anlegen ("+") in der Liste
    // (Wohnung wird beim Anlegen ausgewählt) — wie auf der Wohnung-Detailseite.
    let entry: Partial<WalterVertragEntry> = {
        versionen: [{} as WalterVertragVersionEntry]
    };
</script>

<WalterHeader title="Verträge" />
<WalterVertraege fetchImpl={data.fetchImpl} {entry} fullHeight />
