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

	export let a: Partial<WalterBetriebskostenrechnungEntry> = {};

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
		updateUmlageEntries(a.typ?.id);
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
		a.umlage = undefined;
	}

	function selectUmlage(e: CustomEvent) {
		a.umlage = e.detail.selectedItem;
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
			selectedId={a.umlage?.id}
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
		bind:value={a.betreffendesJahr}
		hideSteppers={false}
		label="Betreffendes Jahr"
	/>
	<WalterNumberInput bind:value={a.betrag} label="Betrag" />
	<WalterDatePicker bind:value={a.datum} labelText="Datum" />
</Row>
<Row>
	<WalterTextInput bind:value={a.notiz} labelText="Notiz" />
</Row>
