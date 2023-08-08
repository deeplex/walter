<script lang="ts">
    import { page } from '$app/stores';
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterBetriebskostenrechnung
    } from '$walter/components';
    import type { WalterBetriebskostenrechnungEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import { onMount } from 'svelte';
    import type { PageData } from './$types';
    import { walter_selection } from '$walter/services/requests';

    export let data: PageData;

    const entry: Partial<WalterBetriebskostenrechnungEntry> = {
        datum: convertDateCanadian(new Date())
    };
    let searchParams: URLSearchParams = new URL($page.url).searchParams;

    onMount(async () => {
        const betriebskostenTypId = searchParams.get('typ');

        const betriebskostentypen = await walter_selection.betriebskostentypen(
            data.fetchImpl
        );
        const umlagen_wohnungen = await walter_selection.umlagen_wohnungen(
            data.fetchImpl
        );

        if (betriebskostenTypId) {
            entry.typ = betriebskostentypen.find(
                (e: any) => +e.id === +betriebskostenTypId
            );
        }

        const umlageId = searchParams.get('umlage');
        if (umlageId) {
            entry.umlage = umlagen_wohnungen.find((e) => +e.id === +umlageId);
        }

        const jahr = searchParams.get('jahr');
        if (jahr) {
            entry.betreffendesJahr = +jahr;
        } else {
            entry.betreffendesJahr = new Date().getFullYear() - 1;
        }
    });
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterBetriebskostenrechnung fetchImpl={data.fetchImpl} {entry} />
</WalterGrid>
