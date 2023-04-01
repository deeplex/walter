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
	import type {
		WalterBetriebskostenabrechnungKostengruppenEntry,
		WalterS3File
	} from '$WalterTypes';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import {
		getMieteEntry,
		getMietminderungEntry,
		getVertragversionEntry,
		loadAbrechnung
	} from './utils';
	import {
		WalterToastContent,
		type WalterMieteEntry,
		type WalterMietminderungEntry,
		type WalterPersonEntry,
		type WalterVertragVersionEntry
	} from '$WalterLib';
	import {
		create_walter_s3_file_from_file,
		walter_s3_post
	} from '$WalterServices/s3';
	import {
		create_abrechnung_pdf,
		create_abrechnung_word
	} from '$WalterServices/abrechnung';

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

	function addToAnhang(file: WalterS3File) {
		if (data.anhaenge.some((e) => e.FileName == file.FileName)) {
			return;
		}
		data.anhaenge = [...data.anhaenge, file];
	}

	async function create_abrechnung(abrechnung: File) {
		const file = create_walter_s3_file_from_file(abrechnung, data.S3URL);

		const toast = new WalterToastContent(
			'Hochladen erfolgreich',
			'Hochladen fehlgeschlagen',
			() => `Die Datei: ${file.FileName} wurde erfolgreich hochgeladen`,
			() => `Die Datei: ${file.FileName} konnte nicht hochgeladen werden.`
		);

		var response = await walter_s3_post(
			new File([abrechnung], file.FileName),
			data.S3URL,
			toast
		);

		if (response.ok) {
			addToAnhang(file);
		}
	}

	async function word_dokument_erstellen_click() {
		const abrechnung = await create_abrechnung_word(data.id, year, title);
		if (abrechnung instanceof File) {
			create_abrechnung(abrechnung);
		}
	}

	async function pdf_dokument_erstellen_click() {
		const abrechnung = await create_abrechnung_pdf(data.id, year, title);
		if (abrechnung instanceof File) {
			create_abrechnung(abrechnung);
		}
	}
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	bind:files={data.anhaenge}
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
			<Button on:click={word_dokument_erstellen_click}
				>Word-Dokument erstellen</Button
			>
			<Button on:click={pdf_dokument_erstellen_click}
				>PDF-Dokument erstellen</Button
			>
		</ButtonSet>
	</Row>

	{#if abrechnung}
		<WalterAbrechnung {abrechnung} />
	{/if}
</WalterGrid>
