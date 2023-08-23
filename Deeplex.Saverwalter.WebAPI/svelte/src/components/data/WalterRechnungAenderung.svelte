<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { BarChartSimple } from '@carbon/charts-svelte'
    import type { WalterBetriebskostenabrechnungEntry, WalterRechnungEntry } from '$walter/types';
    import { convertToData } from './WalterRechnungAenderung';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
    const rechnungen = abrechnung.abrechnungseinheiten.flatMap(einheit => einheit.rechnungen);

    const data = convertToData(rechnungen);
	const options = {
		legend: { enabled: false },
		title: 'Änderung der Rechnungen',
		height: '400px',
		axes: {
			bottom: { mapsTo: 'value', title: 'Betrag'  },
			left: { mapsTo: 'group', scaleType: 'labels', title: 'Rechnung' }
		}
	} as any; // TODO scaleType is not correct
</script>

<BarChartSimple {data} {options} />
