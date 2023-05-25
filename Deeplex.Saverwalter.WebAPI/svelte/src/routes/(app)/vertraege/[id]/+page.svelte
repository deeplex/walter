<script lang="ts">
    import { Truncate } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    import {
        WalterKontakte,
        WalterMieten,
        WalterMietminderungen,
        WalterHeaderDetail,
        WalterGrid,
        WalterVertrag,
        WalterVertragVersionen,
        WalterAbrechnung,
        WalterAbrechnungControl
    } from '$walter/components';
    import type { WalterBetriebskostenabrechnungKostengruppenEntry } from '$walter/types';
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import {
        getMieteEntry,
        getMietminderungEntry,
        getVertragversionEntry
    } from './utils';
    import type {
        WalterMieteEntry,
        WalterMietminderungEntry,
        WalterPersonEntry,
        WalterVertragVersionEntry
    } from '$walter/lib';
    import { loadAbrechnung } from '$walter/services/abrechnung';
    import WalterLinks from '../../../../components/subdetails/WalterLinks.svelte';
    export let data: PageData;

    const ver = data.a.versionen;
    const mietminderungEntry: Partial<WalterMietminderungEntry> =
        getMietminderungEntry(`${data.id}`);
    const vertragversionEntry: Partial<WalterVertragVersionEntry> =
        getVertragversionEntry(`${data.id}`, ver[ver.length - 1]);
    const mieteEntry: Partial<WalterMieteEntry> = getMieteEntry(
        `${data.id}`,
        ver[ver.length - 1]
    );
    const mieterEntry: Partial<WalterPersonEntry> = {};

    let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;
    let searchParams: URLSearchParams = new URL($page.url).searchParams;

    onMount(async () => {
        const year = searchParams.get('abrechnung');
        if (year) {
            abrechnung = await loadAbrechnung(data.id, year, data.fetch);
        }
    });

    let title = `${data.a.wohnung?.text} - ${data.a.mieter
        ?.map((mieter) => mieter.name)
        .join(', ')}`;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    bind:files={data.anhaenge}
    bind:a={data.a}
    apiURL={data.apiURL}
    {title}
    f={data.fetch}
/>

<WalterGrid>
    <WalterVertrag
        kontakte={data.kontakte}
        wohnungen={data.wohnungen}
        bind:a={data.a}
    />

    <WalterLinks>
        <WalterKontakte a={mieterEntry} title="Mieter" rows={data.a.mieter} />
        <WalterVertragVersionen
            a={vertragversionEntry}
            title="Versionen:"
            rows={data.a.versionen}
        />
        <WalterMieten a={mieteEntry} title="Mieten" rows={data.a.mieten} />
        <WalterMietminderungen
            a={mietminderungEntry}
            title="Mietminderungen"
            rows={data.a.mietminderungen}
        />
    </WalterLinks>

    <hr style="margin: 2em" />
    <Truncate>Betriebskostenabrechnung :</Truncate>

    <WalterAbrechnungControl
        {title}
        vertragId={data.id}
        fetchImpl={data.fetch}
        S3URL={data.S3URL}
        firstYear={new Date(data.a.beginn).getFullYear()}
        bind:S3files={data.anhaenge}
        bind:abrechnung
    />

    {#if abrechnung}
        <WalterAbrechnung {abrechnung} />
    {/if}
</WalterGrid>
