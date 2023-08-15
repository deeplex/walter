<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehler,
        WalterZaehlerstaende,
        WalterUmlagen,
        WalterLinks,
        WalterLink
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterS3FileWrapper,
        type WalterZaehlerstandEntry
    } from '$walter/lib';

    export let data: PageData;

    const staende = data.entry.staende;

    const lastZaehlerstand = staende.length
        ? staende[staende.length - 1]
        : undefined;
    const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
        zaehler: { id: '' + data.entry.id, text: data.entry.kennnummer },
        datum: convertDateCanadian(new Date()),
        stand: lastZaehlerstand?.stand || 0,
        einheit: lastZaehlerstand?.einheit
    };

    const title = data.entry.kennnummer;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack()
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterZaehler fetchImpl={data.fetchImpl} entry={data.entry} />

    <WalterLinks>
        <WalterZaehlerstaende
            entry={zaehlerstandEntry}
            title="Zählerstände"
            rows={data.entry.staende}
        />
        <WalterUmlagen
            title="Umlagen"
            fetchImpl={data.fetchImpl}
            rows={data.entry.umlagen}
        />

        {#if data.entry.adresse !== null}
            <WalterLink
                bind:fileWrapper
                name={`Adresse: ${data.entry.adresse?.anschrift}`}
                href={`/adressen/${data.entry.adresse?.id}`}
            />
        {/if}
        {#if data.entry.wohnung !== null}
            <WalterLink
                bind:fileWrapper
                name={`Wohnung: ${data.entry.wohnung?.text}`}
                href={`/wohnungen/${data.entry.wohnung?.id}`}
            />
        {/if}
    </WalterLinks>
</WalterGrid>
