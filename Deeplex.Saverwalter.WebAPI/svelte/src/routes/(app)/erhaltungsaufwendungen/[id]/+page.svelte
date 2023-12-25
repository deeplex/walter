<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterErhaltungsaufwendung,
        WalterLinkTile,
        WalterLinks
    } from '$walter/components';
    import { WalterFileWrapper } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    let title = data.entry.aussteller?.text + ' - ' + data.entry.bezeichnung;
    $: {
        title = data.entry.aussteller?.text + ' - ' + data.entry.bezeichnung;
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
    <WalterErhaltungsaufwendung fetchImpl={data.fetchImpl} entry={data.entry} />

    <WalterLinks>
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.wohnung(`${data.entry.wohnung.id}`)}
            name={`Wohnung: ${data.entry.wohnung.text}`}
            href={`/wohnungen/${data.entry.wohnung.id}`}
        />
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.kontakt(`${data.entry.aussteller.id}`)}
            name={`Aussteller: ${data.entry.aussteller.text}`}
            href={`/kontakte/${data.entry.aussteller.id}`}
        />
    </WalterLinks>
</WalterGrid>
