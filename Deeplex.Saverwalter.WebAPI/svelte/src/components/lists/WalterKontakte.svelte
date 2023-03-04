<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterTextInput } from '$WalterComponents';
	import type {
		WalterJuristischePersonEntry,
		WalterNatuerlichePersonEntry,
		WalterPersonEntry
	} from '$WalterTypes';
	import WalterPerson from '../subdetails/WalterPerson.svelte';
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

	export let rows: Promise<WalterPersonEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let entry:
		| Partial<WalterNatuerlichePersonEntry & WalterJuristischePersonEntry>
		| undefined = undefined;
	let personType: number = 0;
</script>

<WalterDataWrapper
	{addUrl}
	addEntry={entry}
	{title}
	{search}
	{navigate}
	{rows}
	{headers}
>
	{#if entry}
		<Row>
			<ContentSwitcher bind:selectedIndex={personType}>
				<Switch text="Natürliche Person" />
				<Switch text="Juristische Person" />
			</ContentSwitcher>
		</Row>
		<Row>
			{#if personType === 0}
				<WalterTextInput bind:binding={entry.vorname} labelText="Vorname" />
				<WalterTextInput bind:binding={entry.nachname} labelText="Nachname" />
			{:else}
				<WalterTextInput bind:binding={entry.name} labelText="Bezeichnung" />
			{/if}
		</Row>
		<WalterPerson binding={entry} />
	{/if}
</WalterDataWrapper>
