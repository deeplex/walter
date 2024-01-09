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
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterLinks,
        WalterUmlagetyp,
        WalterUmlagen
    } from '$walter/components';
    import { WalterFileWrapper } from '$walter/lib';

    export let data: PageData;

    let title = data.entry.bezeichnung;
    $: {
        title = data.entry.bezeichnung;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.bezeichnung}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterUmlagetyp fetchImpl={data.fetchImpl} bind:entry={data.entry} />
    <WalterLinks>
        <WalterUmlagen
            fetchImpl={data.fetchImpl}
            title="Umlagen"
            rows={data.entry.umlagen}
        />
    </WalterLinks>
</WalterGrid>
