<script lang="ts">
	import { goto } from '$app/navigation';
	import {
		WalterAbrechnungEinheit,
		WalterAbrechnungGruppe,
		WalterAbrechnungResultat,
		WalterDatePicker,
		WalterNumberInput
	} from '$WalterComponents';
	import { getKostenpunkt } from '$WalterServices/abrechnung';
	import { convertEuro } from '$WalterServices/utils';
	import type {
		WalterBetriebskostenabrechnungEntry,
		WalterBetriebskostenabrechnungsRechnungsgruppeEntry
	} from '$WalterTypes';
	import { Row, Tile } from 'carbon-components-svelte';
	import { onMount } from 'svelte';

	export let abrechnung: WalterBetriebskostenabrechnungEntry;

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

	let kostengruppen: WalterBetriebskostenabrechnungsRechnungsgruppeEntry[];
	onMount(() => {
		kostengruppen = abrechnung.gruppen.map((e) => {
			const kostenpunkte = e.umlagen.map((u, i) =>
				getKostenpunkt(
					i,
					u,
					new Date(abrechnung.nutzungsbeginn).toLocaleDateString('de-De'),
					new Date(abrechnung.nutzungsende).toLocaleDateString('de-De'),
					abrechnung.jahr,
					e.wfZeitanteil
				)
			);

			return {
				kostenpunkte,
				...e
			};
		});
	});

	let navigate = (e: CustomEvent<number | null>) => {
		goto(`${e.detail}`, { noScroll: true });
	};
</script>

<Row>
	<WalterDatePicker
		placeholder="Nutzungsbeginn"
		labelText="Nutzungsbeginn"
		disabled
		value={abrechnung.nutzungsbeginn.toLocaleString('de-DE')}
	/>
	<WalterDatePicker
		placeholder="Nutzungsende"
		labelText="Nutzungsende"
		disabled
		value={abrechnung.nutzungsende.toLocaleString('de-DE')}
	/>
</Row>

<WalterAbrechnungResultat entry={abrechnung} />
{#if kostengruppen}
	{#each kostengruppen as gruppe}
		<hr />
		<WalterAbrechnungEinheit entry={gruppe} />
		<WalterAbrechnungGruppe rows={gruppe.kostenpunkte} />
		<Tile light>
			<h5 style="display: flex; justify-content: center">
				Zwischensumme: {convertEuro(gruppe.betragKalt)}
			</h5>
		</Tile>
	{/each}
{/if}
