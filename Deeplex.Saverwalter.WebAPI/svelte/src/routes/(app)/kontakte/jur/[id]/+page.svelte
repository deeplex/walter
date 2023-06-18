<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterKontakte,
        WalterGrid,
        WalterWohnungen,
        WalterVertraege,
        WalterHeaderDetail,
        WalterJuristischePerson,
        WalterLinks
    } from '$walter/components';
    import type {
        WalterJuristischePersonEntry,
        WalterNatuerlichePersonEntry
    } from '$walter/lib';

    export let data: PageData;

    let mitglied: Partial<
        WalterNatuerlichePersonEntry | WalterJuristischePersonEntry
    > = {
        selectedJuristischePersonen: [{ id: +data.id, text: data.entry.name }]
    };

    let juristischePerson: Partial<WalterJuristischePersonEntry> = {
        selectedMitglieder: [{ id: +data.id, text: data.entry.name }]
    };
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.files}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.name}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterJuristischePerson
        entry={data.entry}
        kontakte={data.kontakte}
        juristischePersonen={data.juristischePersonen}
    />

    <WalterLinks>
        <WalterKontakte
            bind:entry={mitglied}
            title="Mitglieder"
            rows={data.entry.mitglieder}
        />
        <WalterKontakte
            bind:entry={juristischePerson}
            title="Juristische Personen"
            rows={data.entry.juristischePersonen}
        />
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterVertraege
            wohnungen={data.wohnungen}
            kontakte={data.kontakte}
            title="Verträge"
            rows={data.entry.vertraege}
        />
    </WalterLinks>
</WalterGrid>
