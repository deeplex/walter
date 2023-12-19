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
