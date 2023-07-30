<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterKontakte,
        WalterGrid,
        WalterWohnungen,
        WalterVertraege,
        WalterHeaderDetail,
        WalterJuristischePerson,
        WalterLinks,
        WalterLink
    } from '$walter/components';
    import {
        WalterS3FileWrapper,
        type WalterJuristischePersonEntry,
        type WalterNatuerlichePersonEntry
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

    const title = data.entry.name;
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
    <WalterJuristischePerson entry={data.entry} fetchImpl={data.fetchImpl} />

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
            fetchImpl={data.fetchImpl}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterVertraege
            fetchImpl={data.fetchImpl}
            title="Verträge"
            rows={data.entry.vertraege}
        />

        <WalterLink
            bind:fileWrapper
            name={`Adresse: ${data.entry.adresse.anschrift}`}
            href={`/adressen/${data.entry.adresse.id}`}
        />
    </WalterLinks>
</WalterGrid>
