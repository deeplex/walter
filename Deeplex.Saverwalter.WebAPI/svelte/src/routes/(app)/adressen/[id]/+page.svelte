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

    let fileWrapper = new WalterS3FileWrapper(data.fetch);
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    bind:fileWrapper
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
/>

<WalterGrid>
    <WalterAdresse bind:value={data.entry} />

    <WalterLinks>
        <WalterWohnungen
            kontakte={data.kontakte}
            entry={wohnungEntry}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterKontakte title="Personen" rows={data.entry.kontakte} />
        <WalterZaehlerList
            umlagen={data.umlagen}
            zaehlertypen={data.zaehlertypen}
            wohnungen={data.wohnungen}
            title="Zähler"
            rows={data.entry.zaehler}
        />
    </WalterLinks>
</WalterGrid>
