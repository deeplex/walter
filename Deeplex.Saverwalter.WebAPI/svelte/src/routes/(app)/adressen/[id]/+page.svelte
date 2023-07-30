<script lang="ts">
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderDetail,
        WalterKontakte,
        WalterLinks,
        WalterWohnungen,
        WalterZaehlerList
    } from '$walter/components';
    import { WalterS3FileWrapper, type WalterWohnungEntry } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;
    const wohnungEntry: Partial<WalterWohnungEntry> = {
        adresse: { ...data.entry }
    };

    const title = data.entry.anschrift;

    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack()
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    bind:fileWrapper
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
/>

<WalterGrid>
    <WalterAdresse bind:entry={data.entry} />

    <WalterLinks>
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            entry={wohnungEntry}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterKontakte title="Personen" rows={data.entry.kontakte} />
        <WalterZaehlerList
            fetchImpl={data.fetchImpl}
            title="Zähler"
            rows={data.entry.zaehler}
        />
    </WalterLinks>
</WalterGrid>
