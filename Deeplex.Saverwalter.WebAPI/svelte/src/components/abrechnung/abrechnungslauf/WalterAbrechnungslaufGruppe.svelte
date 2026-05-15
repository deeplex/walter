<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import type { AbrechnungslaufGruppeResult } from './AbrechnungslaufTypes';
    import WalterAbrechnungslaufUebersicht from './WalterAbrechnungslaufUebersicht.svelte';
    import WalterAbrechnungslaufCharts from './WalterAbrechnungslaufCharts.svelte';
    import WalterAbrechnungslaufBetriebskosten from './WalterAbrechnungslaufBetriebskosten.svelte';
    import WalterAbrechnungslaufVertrag from './WalterAbrechnungslaufVertrag.svelte';
    import { Tab, Tabs } from 'carbon-components-svelte';

    export let gruppe: AbrechnungslaufGruppeResult;
    export let jahr: number;
    export let fetchImpl: typeof fetch;

    let selectedTab = 0;

    const splitWohnungBezeichnung = (value: string) => {
        const separator = ' – ';
        const splitAt = value.indexOf(separator);
        if (splitAt === -1) return { adresse: 'Ohne Adresse', wohnung: value };
        return {
            adresse: value.slice(0, splitAt),
            wohnung: value.slice(splitAt + separator.length)
        };
    };

    $: vertragResultate = [...gruppe.resultate]
        .filter((r) => r.vertragId != null)
        .sort((a, b) => {
            const aW = splitWohnungBezeichnung(a.wohnungBezeichnung);
            const bW = splitWohnungBezeichnung(b.wohnungBezeichnung);
            const adressCompare = aW.adresse.localeCompare(bW.adresse, 'de-DE');
            if (adressCompare !== 0) return adressCompare;
            const wohnungCompare = aW.wohnung.localeCompare(bW.wohnung, 'de-DE');
            if (wohnungCompare !== 0) return wohnungCompare;
            const nutzungVonCompare = a.nutzungVon.localeCompare(b.nutzungVon);
            if (nutzungVonCompare !== 0) return nutzungVonCompare;
            const aBis = a.nutzungBis ?? '9999-12-31';
            const bBis = b.nutzungBis ?? '9999-12-31';
            return aBis.localeCompare(bBis);
        });

    $: hasNkZeilen = gruppe.abrechnungseinheiten.flatMap((e) => e.nkZeilen).length > 0;
</script>

<WalterAbrechnungslaufUebersicht {gruppe} {jahr} />

{#if hasNkZeilen}
    <WalterAbrechnungslaufCharts {gruppe} />
{/if}

<div style="margin-bottom: 0.5rem;">
    <Tabs bind:selected={selectedTab} type="container">
        <Tab label="Betriebskosten" />
        {#each vertragResultate as r}
            <Tab
                label={splitWohnungBezeichnung(r.wohnungBezeichnung).wohnung +
                    ' – ' +
                    r.mieterBezeichnung}
            />
        {/each}
    </Tabs>
</div>

{#if selectedTab === 0}
    <WalterAbrechnungslaufBetriebskosten {gruppe} />
{:else}
    {@const resultat = vertragResultate[selectedTab - 1]}
    {#if resultat}
        <WalterAbrechnungslaufVertrag {resultat} {gruppe} {jahr} {fetchImpl} />
    {/if}
{/if}

<style>
    :global(.bx--tabs--container .bx--tab) {
        flex-shrink: 0;
        max-width: none;
    }
    :global(.bx--tabs--container .bx--tabs__nav-link) {
        font-size: 0.875rem !important;
        font-weight: 600 !important;
        height: auto !important;
        max-width: none !important;
        min-height: 3rem;
        overflow: visible !important;
        white-space: nowrap !important;
        width: auto !important;
    }
</style>
