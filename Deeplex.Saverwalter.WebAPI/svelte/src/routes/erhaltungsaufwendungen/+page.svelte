<script lang="ts">
	import {
		WalterErhaltungsaufwendungen,
		WalterHeaderList
	} from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Erhaltungsaufwendungen" />
<WalterErhaltungsaufwendungen search rows={data.rows} />
