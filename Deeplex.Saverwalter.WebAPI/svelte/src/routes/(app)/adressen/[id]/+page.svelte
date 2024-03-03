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
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderDetail,
        WalterKontakte,
        WalterLinks,
        WalterTextArea,
        WalterWohnungen,
        WalterZaehlerList
    } from '$walter/components';
    import { WalterFileWrapper, type WalterWohnungEntry } from '$walter/lib';
    import { Column, Row } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import WalterDataPieChart from '$walter/components/data/WalterDataPieChart.svelte';
    import {
        walter_data_ne,
        walter_data_nf,
        walter_data_wf
    } from '$walter/components/data/WalterData';

    export let data: PageData;
    const wohnungEntry: Partial<WalterWohnungEntry> = {
        adresse: { ...data.entry }
    };

    let title = data.entry.anschrift;
    $: {
        title = data.entry.anschrift;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    bind:fileWrapper
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
/>

<WalterGrid>
    <WalterAdresse bind:entry={data.entry} />
    <Row>
        <WalterTextArea bind:value={data.entry.notiz} labelText="Notiz" />
    </Row>

    <WalterLinks>
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            entry={wohnungEntry}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterKontakte
            fetchImpl={data.fetchImpl}
            title="Personen"
            rows={data.entry.kontakte}
        />
        <WalterZaehlerList
            fetchImpl={data.fetchImpl}
            title="Zähler"
            rows={data.entry.zaehler}
        />
    </WalterLinks>

    {#if data.entry.wohnungen?.length > 1}
        <Row>
            <Column>
                <WalterDataPieChart
                    config={walter_data_wf('Wohnfläche', data.entry.wohnungen)}
                />
            </Column>
            <Column>
                <WalterDataPieChart
                    config={walter_data_nf('Nutzfläche', data.entry.wohnungen)}
                />
            </Column>
            <Column>
                <WalterDataPieChart
                    config={walter_data_ne(
                        'Nutzeinheiten',
                        data.entry.wohnungen
                    )}
                />
            </Column>
        </Row>
    {/if}
</WalterGrid>
