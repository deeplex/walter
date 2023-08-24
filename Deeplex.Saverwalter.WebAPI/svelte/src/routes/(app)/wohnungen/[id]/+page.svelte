<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterWohnungen,
        WalterVertraege,
        WalterZaehlerList,
        WalterErhaltungsaufwendungen,
        WalterBetriebskostenrechnungen,
        WalterUmlagen,
        WalterHeaderDetail,
        WalterWohnung,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterS3FileWrapper,
        type WalterErhaltungsaufwendungEntry,
        type WalterUmlageEntry,
        type WalterZaehlerEntry
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';
    import { walter_data_aufwendungen } from '$walter/components/data/WalterData';
    import WalterDataScatterChart from '$walter/components/data/WalterDataScatterChart.svelte';

    export let data: PageData;

    const zaehlerEntry: Partial<WalterZaehlerEntry> = {
        wohnung: {
            id: `${data.entry.id}`,
            text: `${data.entry.adresse?.anschrift} - ${data.entry.bezeichnung}`
        },
        adresse: data.entry.adresse ? { ...data.entry.adresse } : undefined
    };
    const umlageEntry: Partial<WalterUmlageEntry> = {
        selectedWohnungen: [
            {
                id: '' + data.entry.id,
                text:
                    data.entry.adresse?.anschrift +
                    ' - ' +
                    data.entry.bezeichnung
            }
        ]
    };
    const erhaltungsaufwendungEntry: Partial<WalterErhaltungsaufwendungEntry> =
        {
            wohnung: {
                id: '' + data.entry.id,
                text:
                    data.entry.adresse?.anschrift +
                    ' - ' +
                    data.entry.bezeichnung
            },
            datum: convertDateCanadian(new Date())
        };

    const title =
        data.entry.adresse?.anschrift + ' - ' + data.entry.bezeichnung;
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
    <WalterWohnung fetchImpl={data.fetchImpl} entry={data.entry} />

    <WalterLinks>
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            title="Wohnungen an der selben Adresse"
            rows={data.entry.haus || []}
        />
        <WalterZaehlerList
            fetchImpl={data.fetchImpl}
            entry={zaehlerEntry}
            title="Zähler"
            rows={data.entry.zaehler}
        />
        <WalterVertraege
            fetchImpl={data.fetchImpl}
            title="Verträge"
            rows={data.entry.vertraege}
        />
        <WalterUmlagen
            fetchImpl={data.fetchImpl}
            entry={umlageEntry}
            title="Umlagen"
            rows={data.entry.umlagen}
        />
        <WalterBetriebskostenrechnungen
            fetchImpl={data.fetchImpl}
            title="Betriebskostenrechnungen"
            rows={data.entry.betriebskostenrechnungen}
        />
        <WalterErhaltungsaufwendungen
            fetchImpl={data.fetchImpl}
            entry={erhaltungsaufwendungEntry}
            title="Erhaltungsaufwendungen"
            rows={data.entry.erhaltungsaufwendungen}
        />

        <WalterLinkTile
            bind:fileWrapper
            name={`Adresse: ${data.entry.adresse.anschrift}`}
            href={`/adressen/${data.entry.adresse.id}`}
        />

        <!-- TODO besitzer id is guid -->
    </WalterLinks>
    
    {#if data.entry.erhaltungsaufwendungen.length > 1}
    <Row>
        <WalterDataScatterChart
            config={walter_data_aufwendungen(
                "Erhaltungsaufwendungen",
                data.entry.erhaltungsaufwendungen)} />
    </Row>
    {/if}
</WalterGrid>
