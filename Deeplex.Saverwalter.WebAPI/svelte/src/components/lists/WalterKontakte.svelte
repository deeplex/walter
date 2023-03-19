<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import {
		WalterDataWrapper,
		WalterPerson,
		WalterTextInput
	} from '$WalterComponents';
	import type {
		WalterJuristischePersonEntry,
		WalterNatuerlichePersonEntry,
		WalterPersonEntry,
		WalterSelectionEntry
	} from '$WalterTypes';
	import { ContentSwitcher, Row, Switch } from 'carbon-components-svelte';

	const headers = [
		{ key: 'name', value: 'Name', default: '' },
		{ key: 'adresse.anschrift', value: 'Anschrift' },
		{ key: 'telefon', value: 'Telefon' },
		{ key: 'mobil', value: 'Mobil' },
		{ key: 'email', value: 'E-Mail' }
	];

	const addUrl = `/api/kontakte/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(
			`/kontakte/${e.detail.id > 0 ? 'nat' : 'jur'}/${Math.abs(e.detail.id)}`
		);

	export let rows: WalterPersonEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	let personType: number = 0;

	export let a:
		| Partial<WalterNatuerlichePersonEntry & WalterJuristischePersonEntry>
		| undefined = undefined;
</script>

<WalterDataWrapper
	{addUrl}
	addEntry={a}
	{title}
	{search}
	{navigate}
	{rows}
	{headers}
>
	{#if a}
		<Row>
			<ContentSwitcher bind:selectedIndex={personType}>
				<Switch text="Natürliche Person" />
				<Switch text="Juristische Person" />
			</ContentSwitcher>
		</Row>
		<Row>
			{#if personType === 0}
				<WalterTextInput bind:value={a.vorname} labelText="Vorname" />
				<WalterTextInput bind:value={a.nachname} labelText="Nachname" />
			{:else}
				<WalterTextInput bind:value={a.name} labelText="Bezeichnung" />
			{/if}
		</Row>
		<WalterPerson value={a} />
	{/if}
</WalterDataWrapper>
