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
