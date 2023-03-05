<script lang="ts">
	import {
		WalterErhaltungsaufwendungen,
		WalterHeaderList
	} from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Erhaltungsaufwendungen" />
<WalterErhaltungsaufwendungen
	kontakte={data.kontakte}
	wohnungen={data.wohnungen}
	search
	rows={data.rows}
/>
