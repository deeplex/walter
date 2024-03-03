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
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import { AccordionItem } from 'carbon-components-svelte';
    import {
        walter_data_rechnungen_pairs,
        walter_data_rechnungen,
        walter_data_rechnungen_diff
    } from './WalterData';
    import WalterDataPieChart from './WalterDataPieChart.svelte';
    import WalterDataBarChartSimple from './WalterDataBarChartSimple.svelte';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;

    const rechnungen = abrechnung.abrechnungseinheiten.flatMap(
        (einheit) => einheit.rechnungen
    );
    const rechnungenFlat = walter_data_rechnungen('Rechnungen', rechnungen);
    const rechnungenDiff = walter_data_rechnungen_diff(
        'Rechnungen im Vergleich zum Vorjahr',
        rechnungen
    );
    const rechnungenBeforeAfter = walter_data_rechnungen_pairs(
        'Differenz zum Vorjahr',
        rechnungen
    );
</script>

<AccordionItem title="Übersicht">
    <WalterDataPieChart config={rechnungenFlat} />
    <WalterDataBarChartSimple config={rechnungenDiff} />
    <WalterDataBarChartSimple config={rechnungenBeforeAfter} />
</AccordionItem>
