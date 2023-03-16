<script lang="ts">
	import type { PageData } from './$types';

	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterAbrechnungGruppe
	} from '$WalterComponents';
	import { getKostenpunkt } from '$WalterServices/abrechnung';
	import { onMount } from 'svelte';
	import type { WalterBetriebskostenabrechnungKostenpunkt } from '$WalterTypes';

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

	const title = 'Vertrag - Betriebskostenrechnung';

	console.log(data.a);

	let kostengruppen: WalterBetriebskostenabrechnungKostenpunkt[][];
	onMount(() => {
		kostengruppen = data.a.gruppen.map((e) =>
			e.umlagen.map((u, i) =>
				getKostenpunkt(
					i,
					u,
					new Date(data.a.nutzungsbeginn).toLocaleDateString('de-De'),
					new Date(data.a.nutzungsende).toLocaleDateString('de-De'),
					+data.year,
					e.wfZeitanteil
				)
			)
		);
		console.log(kostengruppen);
	});
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	files={data.anhaenge}
	bind:a={data.a}
	apiURL={data.apiURL}
	{title}
/>

<WalterGrid>
	{#if kostengruppen}
		{#each kostengruppen as gruppe}
			<WalterAbrechnungGruppe rows={gruppe} />
		{/each}
	{/if}
</WalterGrid>
