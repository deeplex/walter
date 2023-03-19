<script lang="ts">
	import { Accordion, Button, Row, Truncate } from 'carbon-components-svelte';
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
		WalterBetriebskostenabrechnungEntry,
		WalterMieteEntry,
		WalterMietminderungEntry,
		WalterPersonEntry,
		WalterVertragVersionEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';
	import { goto } from '$app/navigation';
	import { walter_get } from '$WalterServices/requests';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import {
		getMieteEntry,
		getMietminderungEntry,
		getVertragversionEntry,
		loadAbrechnung
	} from './utils';

	export let data: PageData;

	const mietminderungEntry: Partial<WalterMietminderungEntry> =
		getMietminderungEntry(`${data.id}`);
	const vertragversionEntry: Partial<WalterVertragVersionEntry> =
		getVertragversionEntry(`${data.id}`, data.a.versionen.pop());
	const mieteEntry: Partial<WalterMieteEntry> = getMieteEntry(
		`${data.id}`,
		data.a.versionen.pop()
	);
	const mieterEntry: Partial<WalterPersonEntry> = {};

	let jahr: number = new Date().getFullYear() - 1;

	let abrechnung: WalterBetriebskostenabrechnungEntry;
	let searchParams: URLSearchParams = new URL($page.url).searchParams;

	onMount(async () => {
		const year = searchParams.get('abrechnung');
		if (year) {
			abrechnung = await loadAbrechnung(data.id, year, data.fetch);
		}
	});

	async function abrechnung_click(id: string, year: number) {
		searchParams = new URLSearchParams({ abrechnung: `${year}` });
		goto(`?${searchParams.toString()}`, { noScroll: true });
		abrechnung = await loadAbrechnung(id, `${year}`, data.fetch);
	}

	let title = `${data.a.wohnung?.text} - ${data.a.mieter
		?.map((m) => m.name)
		.join(', ')}`;
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
	<Truncate>Betriebskostenabrechnung erstellen:</Truncate>

	<Row>
		<WalterNumberInput bind:value={jahr} label="Jahr" hideSteppers={false} />
		<Button size="small" on:click={() => abrechnung_click(data.id, jahr)}>
			Erstellen
		</Button>
	</Row>

	{#if searchParams.has('abrechnung') && abrechnung}
		<WalterAbrechnung bind:abrechnung />
	{/if}
</WalterGrid>
