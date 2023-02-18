<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import WalterDatePicker from '../../../components/WalterDatePicker.svelte';
	import { convertDate, request_options } from '../../../services/utils';
	import type { VertragEntry } from '../../../types/vertrag.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const async: Promise<VertragEntry> = fetch(
		`/api/vertraege/${data.id}`,
		request_options
	).then((e) => e.json());
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<WalterDatePicker value={x.beginn} labelText="Beginn" />
			<WalterDatePicker value={x.ende} labelText="Ende" placeholder="Offen" />
		</Row>
	{/await}
</Grid>
