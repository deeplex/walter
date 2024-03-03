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
    import {
        WalterGrid,
        WalterVertrag,
        WalterHeaderNew,
        WalterVertragVersion
    } from '$walter/components';
    import type {
        WalterVertragEntry,
        WalterVertragVersionEntry
    } from '$walter/lib';
    import { Tile } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;

    let entryVersion: Partial<WalterVertragVersionEntry> = {};
    let entry: Partial<WalterVertragEntry> = {
        versionen: [entryVersion as WalterVertragVersionEntry]
    };
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterVertrag fetchImpl={data.fetchImpl} {entry} />
    <Tile light>Vertragsversion:</Tile>
    <WalterVertragVersion entry={entryVersion} />
</WalterGrid>
