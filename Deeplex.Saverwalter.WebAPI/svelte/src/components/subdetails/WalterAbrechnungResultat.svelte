<script lang="ts">
	import { convertEuro, convertM2 } from '$WalterServices/utils';
	import type { WalterBetriebskostenabrechnungEntry } from '$WalterTypes';
	import {
		Row,
		StructuredList,
		StructuredListBody,
		StructuredListCell,
		StructuredListHead,
		StructuredListRow,
		Tile
	} from 'carbon-components-svelte';

	export let entry: WalterBetriebskostenabrechnungEntry;
</script>

<Row>
	<StructuredList style="margin-right: 2em">
		<StructuredListHead>
			<StructuredListRow head>
				<StructuredListCell head>Teil</StructuredListCell>
				<StructuredListCell head style="text-align:right"
					>Betrag</StructuredListCell
				>
			</StructuredListRow>
		</StructuredListHead>
		<StructuredListBody>
			{#each entry.gruppen as gruppe, index}
				{#if gruppe.betragKalt}
					<StructuredListRow>
						<StructuredListCell
							>Einheit {index + 1} (kalte Nebenkosten) :</StructuredListCell
						>
						<StructuredListCell style="text-align: right"
							>{convertEuro(gruppe.betragKalt || 0)}</StructuredListCell
						>
					</StructuredListRow>
				{/if}
				{#if gruppe.betragWarm}
					<StructuredListRow>
						<StructuredListCell
							>Einheit {index + 1} (warme Nebenkosten) :</StructuredListCell
						>
						<StructuredListCell style="text-align: right"
							>{convertEuro(gruppe.betragWarm)}</StructuredListCell
						>
					</StructuredListRow>
				{/if}
			{/each}
			<StructuredListRow>
				<StructuredListCell>Kaltmiete:</StructuredListCell>
				<StructuredListCell style="text-align: right"
					>{convertEuro(entry.kaltMiete)}</StructuredListCell
				>
			</StructuredListRow>
			<StructuredListRow>
				<StructuredListCell>Gezahlt :</StructuredListCell>
				<StructuredListCell style="text-align: right"
					>{convertEuro(entry.gezahlt)}</StructuredListCell
				>
			</StructuredListRow>
			<StructuredListRow>
				<StructuredListCell head>Ergebnis der Abrechnung :</StructuredListCell>
				<StructuredListCell head style="text-align: right">
					{convertEuro(entry.result)}
				</StructuredListCell>
			</StructuredListRow>
		</StructuredListBody>
	</StructuredList>
</Row>
