<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import type { WalterUmlageEntry, WalterVertragEntry } from '$walter/lib';
    import {
        StructuredList,
        StructuredListBody,
        StructuredListRow,
        Tab,
        Tabs
    } from 'carbon-components-svelte';
    import WalterMiettabelle from './WalterMiettabelle.svelte';
    import WalterRechnungenTabelle from './WalterRechnungenTabelle.svelte';
    import {
        walter_data_miettabelle,
        walter_data_rechnungentabelle
    } from '../data/WalterData';

    export let vertraege: WalterVertragEntry[];
    export let umlagen: WalterUmlageEntry[];
    export let fetchImpl: typeof fetch;

    const mieten = vertraege
        .flatMap((vertrag) => vertrag.mieten)
        .sort(
            (mieteA, mieteB) =>
                new Date(mieteA.betreffenderMonat).getTime() -
                new Date(mieteB.betreffenderMonat).getTime()
        );
    const years: number[] = [];
    for (
        let i = new Date(mieten[0]?.betreffenderMonat).getFullYear();
        i <
        new Date(mieten[mieten.length - 1]?.betreffenderMonat).getFullYear() +
            1;
        ++i
    ) {
        years.push(i);
    }

    let selected = years.findIndex((year) => year === new Date().getFullYear());
    selected = selected === -1 ? years.length - 1 : selected;
</script>

<div>
    <!-- svelte-ignore missing-declaration -->
    <Tabs style="position: fixed; z-index: 3000" bind:selected type="container">
        {#each years as year}
            <Tab label={`${year}`} />
        {/each}
        <svelte:fragment slot="content">
            <div style="height: 5em; width: 100vw; display: block" />
            <StructuredList>
                <StructuredListBody>
                    <StructuredListRow>
                        <WalterMiettabelle
                            config={walter_data_miettabelle(
                                vertraege,
                                years[selected]
                            )}
                            year={years[selected]}
                            {mieten}
                            {vertraege}
                        />
                    </StructuredListRow>
                    <StructuredListRow>
                        <WalterRechnungenTabelle
                            config={walter_data_rechnungentabelle(
                                umlagen,
                                years[selected]
                            )}
                            year={years[selected]}
                            {fetchImpl}
                            {umlagen}
                        />
                    </StructuredListRow>
                </StructuredListBody>
            </StructuredList>
        </svelte:fragment>
    </Tabs>
</div>
