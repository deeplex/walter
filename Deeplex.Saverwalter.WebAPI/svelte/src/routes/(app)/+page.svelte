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
    import { Content } from 'carbon-components-svelte';
    import { WalterHeader, WalterMiettabelleWrapper } from '$walter/components';
    import {
        WalterUmlageEntry,
        WalterVertragEntry,
        type WalterUmlageEntry as WalterUmlageEntryType,
        type WalterVertragEntry as WalterVertragEntryType
    } from '$walter/lib';
    import type { PageData } from './$types';
    import { onMount } from 'svelte';

    export let title = 'Walter';
    export let data: PageData;

    let vertraege: WalterVertragEntryType[] = [];
    let umlagen: WalterUmlageEntryType[] = [];

    let vertraegeReady = false;
    let umlagenReady = false;

    onMount(() => {
        const fetchImpl = data.fetchImpl;

        WalterVertragEntry.GetAll<WalterVertragEntryType>(fetchImpl)
            .then((value) => {
                vertraege = value;
            })
            .catch((error: unknown) => {
                console.error('Konnte Vertraege nicht laden:', error);
            })
            .finally(() => {
                vertraegeReady = true;
            });

        WalterUmlageEntry.GetAll<WalterUmlageEntryType>(fetchImpl)
            .then((value) => {
                umlagen = value;
            })
            .catch((error: unknown) => {
                console.error('Konnte Umlagen nicht laden:', error);
            })
            .finally(() => {
                umlagenReady = true;
            });
    });
</script>

<WalterHeader {title} />
<Content>
    <WalterMiettabelleWrapper
        fetchImpl={data.fetchImpl}
        {umlagen}
        {vertraege}
        {vertraegeReady}
        {umlagenReady}
    />
</Content>
