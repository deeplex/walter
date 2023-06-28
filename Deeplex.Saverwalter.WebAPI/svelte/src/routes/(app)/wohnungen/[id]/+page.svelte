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
        WalterLink
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterS3FileWrapper,
        type WalterErhaltungsaufwendungEntry,
        type WalterUmlageEntry,
        type WalterZaehlerEntry
    } from '$walter/lib';

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
    let fileWrapper = new WalterS3FileWrapper(data.fetch);
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterWohnung kontakte={data.kontakte} entry={data.entry} />

    <WalterLinks>
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen an der selben Adresse"
            rows={data.entry.haus || []}
        />
        <WalterZaehlerList
            zaehlertypen={data.zaehlertypen}
            umlagen={data.umlagen}
            wohnungen={data.wohnungen}
            entry={zaehlerEntry}
            title="Zähler"
            rows={data.entry.zaehler}
        />
        <WalterVertraege
            wohnungen={data.wohnungen}
            kontakte={data.kontakte}
            title="Verträge"
            rows={data.entry.vertraege}
        />
        <WalterUmlagen
            zaehler={data.zaehler}
            wohnungen={data.wohnungen}
            umlageschluessel={data.umlageschluessel}
            betriebskostentypen={data.betriebskostentypen}
            entry={umlageEntry}
            title="Umlagen"
            rows={data.entry.umlagen}
        />
        <WalterBetriebskostenrechnungen
            betriebskostentypen={data.betriebskostentypen}
            umlagen_wohnungen={data.umlagen_wohnungen}
            title="Betriebskostenrechnungen"
            rows={data.entry.betriebskostenrechnungen}
        />
        <WalterErhaltungsaufwendungen
            wohnungen={data.wohnungen}
            kontakte={data.kontakte}
            entry={erhaltungsaufwendungEntry}
            title="Erhaltungsaufwendungen"
            rows={data.entry.erhaltungsaufwendungen}
        />

        <WalterLink
            bind:fileWrapper
            name={`Adresse: ${data.entry.adresse.anschrift}`}
            href={`/adressen/${data.entry.adresse.id}`}
        />

        <!-- TODO besitzer id is guid -->
    </WalterLinks>
</WalterGrid>
