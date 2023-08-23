<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { PieChart } from '@carbon/charts-svelte'
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import { convertToData } from './WalterRechnungVerteilung';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
    const rechnungen = abrechnung.abrechnungseinheiten.flatMap(einheit => einheit.rechnungen);

    const data = convertToData(rechnungen);
    const options = {
		legend: { enabled: false },
		title: 'Rechnungen',
		height: '400px',
	};
</script>

<PieChart {data} {options} />
