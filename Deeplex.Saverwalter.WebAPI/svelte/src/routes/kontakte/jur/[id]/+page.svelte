<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import Person from '../../../../components/Person.svelte';
	import {
		Column,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { JuristischePersonEntry } from '../../../../types/juristischeperson.type';
	import AsyncPerson from '../../../../components/AsyncPerson.svelte';
	import { walter_get } from '../../../../services/utils';

	export let data: PageData;
	const async: Promise<JuristischePersonEntry> = walter_get(
		`/api/kontakte/jur/${data.id}`
	);
</script>

<Grid padding>
	{#await async}
		<Row>
			<Column><TextInputSkeleton /></Column>
		</Row>
		<AsyncPerson />
	{:then person}
		<Row>
			<TextInput required labelText="Bezeichnung" value={person.name} />
		</Row>
		<Person {person} />
	{/await}
</Grid>
