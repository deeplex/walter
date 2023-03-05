<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { ComboBox, Row, TextInputSkeleton } from 'carbon-components-svelte';
	import type {
		WalterBetriebskostenrechnungEntry,
		WalterSelectionEntry
	} from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';

	export let a:
		| Promise<Partial<WalterBetriebskostenrechnungEntry>>
		| undefined = undefined;
	export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};

	const betriebskostentypen: Promise<WalterSelectionEntry[]> = walter_get(
		'/api/selection/betriebskostentypen'
	);
	const umlagePromise: Promise<WalterSelectionEntry[]> = walter_get(
		'/api/selection/umlagen'
	);
	let umlagen: WalterSelectionEntry[];
	let umlageEntries: WalterSelectionEntry[] = [];
	umlagePromise.then((x) => {
		umlagen = x;
		updateUmlageEntries(entry.typ?.id);
	});

	function updateUmlageEntries(id: string | number | undefined) {
		umlageEntries = umlagen.filter((u) => u.filter === id);
	}

	function shouldFilterItem(item: WalterSelectionEntry, value: string) {
		if (!value) return true;
		return item.text.toLowerCase().includes(value.toLowerCase());
	}

	function selectTyp(e: CustomEvent) {
		updateUmlageEntries(e.detail.selectedItem.id);
		entry.umlage = undefined;
	}

	function selectUmlage(e: CustomEvent) {
		entry.umlage = e.detail.selectedItem;
		// entry.typ = entry.umlage.filter;
	}
</script>

<Row>
	{#await betriebskostentypen}
		<TextInputSkeleton />
	{:then items}
		{#await a}
			<TextInputSkeleton />
		{:then x}
			<ComboBox
				selectedId={x?.typ?.id}
				on:select={selectTyp}
				style="padding-right: 1rem"
				{items}
				value={x?.typ?.text}
				titleText="Betriebskostentyp der Umlage"
				{shouldFilterItem}
			/>
		{/await}
	{/await}

	{#await umlagePromise}
		<TextInputSkeleton />
	{:then}
		<ComboBox
			selectedId={entry.umlage?.id}
			on:select={selectUmlage}
			style="padding-right: 1rem"
			bind:items={umlageEntries}
			titleText="Wohnungen der Umlage"
			{shouldFilterItem}
		/>
	{/await}
</Row>

<Row>
	<WalterNumberInput
		bind:binding={entry.betreffendesJahr}
		hideSteppers={false}
		label="Betreffendes Jahr"
		value={a?.then((x) => x.betreffendesJahr)}
	/>
	<WalterNumberInput
		bind:binding={entry.betrag}
		label="Betrag"
		value={a?.then((x) => x.betrag)}
	/>
	<WalterDatePicker
		bind:binding={entry.datum}
		labelText="Datum"
		value={a?.then((x) => x.datum)}
	/>
</Row>
<Row>
	<WalterTextInput
		bind:binding={entry.notiz}
		labelText="Notiz"
		value={a?.then((x) => x.notiz)}
	/>
</Row>
