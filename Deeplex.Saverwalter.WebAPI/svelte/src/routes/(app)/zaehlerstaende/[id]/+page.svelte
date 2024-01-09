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
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehlerstand,
        WalterLinkTile
    } from '$walter/components';
    import { convertDateGerman } from '$walter/services/utils';
    import type { PageData } from './$types';
    import { WalterFileWrapper } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    let title =
        data.entry.zaehler.text +
        ' - ' +
        convertDateGerman(new Date(data.entry.datum));
    $: {
        title =
            data.entry.zaehler.text +
            ' - ' +
            convertDateGerman(new Date(data.entry.datum));
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterZaehlerstand fetchImpl={data.fetchImpl} entry={data.entry} />
    <WalterLinkTile
        bind:fileWrapper
        fileref={fileURL.zaehler(`${data.entry.zaehler.id}`)}
        name={`Zähler: ${data.entry.zaehler.text}`}
        href={`/zaehler/${data.entry.zaehler.id}`}
    />
</WalterGrid>
