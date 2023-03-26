<script lang="ts">
	import {
		Accordion,
		Button,
		ButtonSet,
		Row,
		Truncate
	} from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterKontakte,
		WalterMieten,
		WalterMietminderungen,
		WalterHeaderDetail,
		WalterGrid,
		WalterVertrag,
		WalterVertragVersionen,
		WalterNumberInput,
		WalterAbrechnung
	} from '$WalterComponents';
	import type { WalterBetriebskostenabrechnungKostengruppenEntry } from '$WalterTypes';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import {
		getMieteEntry,
		getMietminderungEntry,
		getVertragversionEntry,
		loadAbrechnung
	} from './utils';
	import type {
		WalterMieteEntry,
		WalterMietminderungEntry,
		WalterPersonEntry,
		WalterVertragVersionEntry
	} from '$WalterLib';
	import {
		create_walter_s3_file_from_file,
		walter_s3_post
	} from '$WalterServices/s3';
	import { create_abrechnung } from '$WalterServices/abrechnung';

	export let data: PageData;

	const ver = data.a.versionen;
	const mietminderungEntry: Partial<WalterMietminderungEntry> =
		getMietminderungEntry(`${data.id}`);
	const vertragversionEntry: Partial<WalterVertragVersionEntry> =
		getVertragversionEntry(`${data.id}`, ver[ver.length - 1]);
	const mieteEntry: Partial<WalterMieteEntry> = getMieteEntry(
		`${data.id}`,
		ver[ver.length - 1]
	);
	const mieterEntry: Partial<WalterPersonEntry> = {};

	let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;
	let searchParams: URLSearchParams = new URL($page.url).searchParams;

	onMount(async () => {
		const year = searchParams.get('abrechnung');
		if (year) {
			abrechnung = await loadAbrechnung(data.id, year, data.fetch);
		}
	});
	let year: number =
		+(searchParams.get('abrechnung') || 0) || new Date().getFullYear() - 1;
	async function abrechnung_click() {
		searchParams = new URLSearchParams({ abrechnung: `${year}` });
		goto(`?${searchParams.toString()}`, { noScroll: true });
		abrechnung = await loadAbrechnung(data.id, `${year}`, data.fetch);
	}

	let title = `${data.a.wohnung?.text} - ${data.a.mieter
		?.map((mieter) => mieter.name)
		.join(', ')}`;

	function dokument_erstellen_click() {
		create_abrechnung(data.id, year, title).then((e) => {
			const file = create_walter_s3_file_from_file(e, data.S3URL);
			walter_s3_post(new File([e], file.FileName), data.S3URL).then((e) => {
				if (e.ok) {
					if (data.anhaenge.some((e) => e.FileName == file.FileName)) {
						return;
					}
					data.anhaenge = [...data.anhaenge, file];
				}
			});
		});
	}
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	files={data.anhaenge}
	bind:a={data.a}
	apiURL={data.apiURL}
	{title}
/>

<WalterGrid>
	<WalterVertrag
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		bind:a={data.a}
	/>

	<Accordion>
		<WalterKontakte a={mieterEntry} title="Mieter" rows={data.a.mieter} />
		<WalterVertragVersionen
			a={vertragversionEntry}
			title="Versionen:"
			rows={data.a.versionen}
		/>
		<WalterMieten a={mieteEntry} title="Mieten" rows={data.a.mieten} />
		<WalterMietminderungen
			a={mietminderungEntry}
			title="Mietminderungen"
			rows={data.a.mietminderungen}
		/>
	</Accordion>

	<hr style="margin: 2em" />
	<Truncate>Betriebskostenabrechnung :</Truncate>

	<Row>
		<WalterNumberInput
			min={new Date(data.a.beginn).getFullYear()}
			bind:value={year}
			label="Jahr"
			hideSteppers={false}
		/>
		<ButtonSet style="margin: auto">
			<Button on:click={abrechnung_click}>Vorschau anzeigen</Button>
			<Button on:click={dokument_erstellen_click}
				>Word-Dokument erstellen</Button
			>
		</ButtonSet>
	</Row>

	{#if abrechnung}
		<WalterAbrechnung {abrechnung} />
	{/if}
</WalterGrid>
