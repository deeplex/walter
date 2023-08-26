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
    import { Column, Row } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import WalterDataPieChart from '$walter/components/data/WalterDataPieChart.svelte';
    import { walter_data_ne, walter_data_nf, walter_data_wf } from '$walter/components/data/WalterData';

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

    {#if data.entry.wohnungen?.length > 1}
    <Row>
        <Column>
            <WalterDataPieChart config={walter_data_wf("Wohnfläche", data.entry.wohnungen)} /> 
        </Column>
        <Column>
            <WalterDataPieChart config={walter_data_nf("Nutzfläche", data.entry.wohnungen)} /> 
        </Column>
        <Column>
            <WalterDataPieChart config={walter_data_ne("Nutzeinheiten", data.entry.wohnungen)} /> 
        </Column>
    </Row>
    {/if}
</WalterGrid>
