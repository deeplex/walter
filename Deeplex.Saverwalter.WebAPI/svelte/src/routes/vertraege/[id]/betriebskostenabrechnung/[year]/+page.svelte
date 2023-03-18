<script lang="ts">
	import type { PageData } from './$types';

	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterAbrechnungGruppe,
		WalterVertrag,
		WalterDatePicker,
		WalterAbrechnungEinheit
	} from '$WalterComponents';
	import { getKostenpunkt } from '$WalterServices/abrechnung';
	import { onMount } from 'svelte';
	import type {
		WalterBetriebskostenabrechnungKostenpunkt,
		WalterBetriebskostenabrechnungsRechnungsgruppeEntry
	} from '$WalterTypes';
	import {
		Row,
		StructuredList,
		StructuredListBody,
		StructuredListCell,
		StructuredListHead,
		StructuredListRow,
		Tile
	} from 'carbon-components-svelte';
	import { convertEuro, convertM2 } from '$WalterServices/utils';

	export let data: PageData;

	// create_abrechnung(id, j, title).then((e) => {
	// 	const file = create_walter_s3_file_from_file(e, data.S3URL);
	// 	walter_s3_post(new File([e], file.FileName), `${data.S3URL}`).then(
	// 		(e) => {
	// 			if (e.ok) {
	// 				data.anhaenge = [...data.anhaenge, file];
	// 			}
	// 		}
	// 	);
	// });

	const wohnungText = data.vertrag.wohnung?.text;
	const mieterText = data.vertrag.mieter?.map((m) => m.name).join(', ');
	let title = `Abrechnung ${data.year}: ${wohnungText} - ${mieterText}`;

	let kostengruppen: WalterBetriebskostenabrechnungsRechnungsgruppeEntry[];
	onMount(() => {
		kostengruppen = data.abrechnung.gruppen.map((e) => {
			const kostenpunkte = e.umlagen.map((u, i) =>
				getKostenpunkt(
					i,
					u,
					new Date(data.abrechnung.nutzungsbeginn).toLocaleDateString('de-De'),
					new Date(data.abrechnung.nutzungsende).toLocaleDateString('de-De'),
					+data.year,
					e.wfZeitanteil
				)
			);

			return {
				kostenpunkte,
				...e
			};
		});
	});
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	files={data.anhaenge}
	bind:a={data.abrechnung}
	apiURL={data.abrechnungURL}
	{title}
/>

<WalterGrid>
	<WalterVertrag
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		a={data.vertrag}
	/>

	<Row>
		<WalterDatePicker
			placeholder="Nutzungsbeginn"
			labelText="Nutzungsbeginn"
			disabled
			value={data.abrechnung.nutzungsbeginn.toLocaleString('de-DE')}
		/>
		<WalterDatePicker
			placeholder="Nutzungsende"
			labelText="Nutzungsende"
			disabled
			value={data.abrechnung.nutzungsende.toLocaleString('de-DE')}
		/>
	</Row>
	<Row>
		<Tile light>
			<h5 style="display: flex; justify-content: center">
				Abrechnungsbetrag: {convertEuro(data.abrechnung.result)}
			</h5>
		</Tile>
	</Row>
	{#if kostengruppen}
		{#each kostengruppen as gruppe}
			<hr />
			<Tile light>
				<h4>Abrechnungseinheit: {gruppe.bezeichnung}</h4>
			</Tile>
			<WalterAbrechnungEinheit entry={gruppe} />
			<WalterAbrechnungGruppe rows={gruppe.kostenpunkte} />
			<Tile light>
				<h5 style="display: flex; justify-content: center">
					Zwischensumme: {convertEuro(gruppe.betragKalt)}
				</h5>
			</Tile>
		{/each}
	{/if}
</WalterGrid>
